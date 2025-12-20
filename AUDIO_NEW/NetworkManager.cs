using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AUDIO_NEW
{
    /// <summary>
    /// Quản lý tất cả các kết nối mạng UDP (Discovery, Signaling)
    /// </summary>
    public class NetworkManager
    {
        private const int LAN_DISCOVERY_PORT = 5000;
        private UdpClient udpBroadcast;
        private UdpClient udpListener;

        public string UserName { get; private set; }
        public string DisplayName { get; private set; }
        public string UserGender { get; private set; }
        public int LocalAudioPort { get; private set; }
        public string LocalIPAddress { get; private set; }

        // ✅ FIX: Thêm unique ID cho mỗi instance
        private string instanceId;

        // Events
        public event EventHandler<PeerInfo> PeerDiscovered;
        public event EventHandler<CallRequestEventArgs> CallRequestReceived;
        public event EventHandler<string> CallAccepted;
        public event EventHandler<string> CallRejected;
        public event EventHandler<string> CallCancelled;
        public event EventHandler<string> CallEnded;

        public NetworkManager(string userName, string displayName, string userGender, int localAudioPort)
        {
            UserName = userName;
            DisplayName = displayName;
            UserGender = userGender;
            LocalAudioPort = localAudioPort;
            LocalIPAddress = GetLocalIPAddress();

            // ✅ FIX: Tạo unique ID (GUID + timestamp)
            instanceId = $"{Guid.NewGuid().ToString().Substring(0, 8)}_{DateTime.Now.Ticks}";
            Debug.WriteLine($"[NETWORK] Instance ID: {instanceId}");
        }

        private string GetLocalIPAddress()
        {
            foreach (var ni in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
            {
                if (ni.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ni.ToString();
                }
            }
            throw new Exception("Không tìm thấy IPv4 của máy!");
        }

        public void StartBroadcast()
        {
            udpBroadcast = new UdpClient();
            udpBroadcast.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            udpBroadcast.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpBroadcast.EnableBroadcast = true;

            Task.Run(async () =>
            {
                while (true)
                {
                    try
                    {
                        
                        string message = $"DISCOVERY|{UserName}|{DisplayName}|{UserGender}|{LocalAudioPort}|{instanceId}";
                        byte[] data = Encoding.UTF8.GetBytes(message);

                        await udpBroadcast.SendAsync(data, data.Length, new IPEndPoint(IPAddress.Broadcast, LAN_DISCOVERY_PORT));
                        await Task.Delay(2000);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[BROADCAST ERROR] " + ex.ToString());
                        await Task.Delay(1000);
                    }
                }
            });
        }

        public void StartListener()
        {
            try
            {
                udpListener = new UdpClient();
                udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpListener.ExclusiveAddressUse = false;

                var localEndPoint = new IPEndPoint(IPAddress.Any, LAN_DISCOVERY_PORT);
                udpListener.Client.Bind(localEndPoint);
                udpListener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, 1);
                udpListener.EnableBroadcast = true;

                Debug.WriteLine($"[LISTENER] Listening on port {LAN_DISCOVERY_PORT}");
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi tạo UDP listener: " + ex.Message);
            }

            Task.Run(() =>
            {
                while (true)
                {
                    try
                    {
                        IPEndPoint remote = new IPEndPoint(IPAddress.Any, 0);
                        byte[] data = udpListener.Receive(ref remote);
                        string message = Encoding.UTF8.GetString(data);

                        Debug.WriteLine($"[RECEIVE] From {remote.Address}:{remote.Port} → {message}");
                        ProcessMessage(message, remote);
                    }
                    catch (ObjectDisposedException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[LISTENER ERROR] " + ex.ToString());
                        Thread.Sleep(500);
                    }
                }
            });
        }

        private void ProcessMessage(string message, IPEndPoint remote)
        {
            if (message.StartsWith("DISCOVERY|"))
            {
                ProcessDiscovery(message, remote);
            }
            else if (message.StartsWith("CALL_REQUEST|"))
            {
                ProcessCallRequest(message, remote);
            }
            else if (message.StartsWith("CALL_ACCEPT|"))
            {
                ProcessCallAccept(message, remote);
            }
            else if (message.StartsWith("CALL_REJECT|"))
            {
                ProcessCallReject(message, remote);
            }
            else if (message.StartsWith("CALL_CANCEL|"))
            {
                ProcessCallCancel(message, remote);
            }
            else if (message.StartsWith("END_CALL|"))
            {
                ProcessEndCall(message, remote);
            }
        }

        private void ProcessDiscovery(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 6) return; //  FIX: Cần 6 parts (thêm instanceId)

            string peerUsername = parts[1];
            string peerDisplayName = parts[2];
            string peerGender = parts[3];
            int peerAudioPort = int.Parse(parts[4]);
            string peerInstanceId = parts[5];

            //  FIX: Chỉ skip nếu là message của CHÍNH MÌNH (dựa vào instanceId)
            if (peerInstanceId == instanceId)
            {
                Debug.WriteLine($"[SKIP] Own message - instanceId: {instanceId}");
                return;
            }

            // ✅ OPTIONAL: Log để debug
            Debug.WriteLine($"[DISCOVERY] Peer: {peerUsername} ({peerDisplayName}) from {remote.Address}");

            IPEndPoint peerEndpoint = new IPEndPoint(remote.Address, peerAudioPort);

            var peerInfo = new PeerInfo
            {
                Username = peerUsername,
                DisplayName = peerDisplayName,
                EndPoint = peerEndpoint,
                Gender = peerGender,
                LastSeen = DateTime.Now
            };

            PeerDiscovered?.Invoke(this, peerInfo);
        }

        private void ProcessCallRequest(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 6) return; // ✅ FIX: Thêm instanceId

            string callerUsername = parts[1];
            string callerDisplayName = parts[2];
            string callerGender = parts[3];
            int callerPort = int.Parse(parts[4]);
            string callerInstanceId = parts[5];

            // ✅ FIX: Skip own call request
            if (callerInstanceId == instanceId)
            {
                Debug.WriteLine($"[SKIP] Own CALL_REQUEST");
                return;
            }

            IPEndPoint callerEndpoint = new IPEndPoint(remote.Address, callerPort);

            var args = new CallRequestEventArgs
            {
                CallerUsername = callerUsername,
                CallerDisplayName = callerDisplayName,
                CallerGender = callerGender,
                CallerEndpoint = callerEndpoint
            };

            CallRequestReceived?.Invoke(this, args);
        }

        private void ProcessCallAccept(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return;

            string acceptUser = parts[1];
            CallAccepted?.Invoke(this, acceptUser);
        }

        private void ProcessCallReject(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return;

            string rejectUser = parts[1];
            CallRejected?.Invoke(this, rejectUser);
        }

        private void ProcessCallCancel(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return;

            string cancelUser = parts[1];
            CallCancelled?.Invoke(this, cancelUser);
        }

        private void ProcessEndCall(string message, IPEndPoint remote)
        {
            var parts = message.Split('|');
            if (parts.Length < 2) return;

            string endUser = parts[1];
            CallEnded?.Invoke(this, endUser);
        }

        public void SendCallRequest(string targetUsername, IPEndPoint peer)
        {
            try
            {
                IPEndPoint signalingPeer = new IPEndPoint(peer.Address, LAN_DISCOVERY_PORT);
                using (UdpClient udp = new UdpClient())
                {
                    // ✅ FIX: Thêm instanceId
                    string message = $"CALL_REQUEST|{UserName}|{DisplayName}|{UserGender}|{LocalAudioPort}|{instanceId}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udp.Send(data, data.Length, signalingPeer);
                    Debug.WriteLine($"[SEND] CALL_REQUEST to {targetUsername}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi gửi yêu cầu gọi: " + ex.Message);
            }
        }

        public void SendCallCancel(string targetUsername, IPEndPoint peer)
        {
            try
            {
                IPEndPoint signalingPeer = new IPEndPoint(peer.Address, LAN_DISCOVERY_PORT);
                using (UdpClient udp = new UdpClient())
                {
                    string message = $"CALL_CANCEL|{UserName}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udp.Send(data, data.Length, signalingPeer);
                    Debug.WriteLine($"[SEND] CALL_CANCEL to {targetUsername}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL ERROR] SendCallCancel: {ex.Message}");
            }
        }

        public void SendCallAccept(IPEndPoint peer, string callerUsername)
        {
            try
            {
                IPEndPoint signalingPeer = new IPEndPoint(peer.Address, LAN_DISCOVERY_PORT);
                using (UdpClient udp = new UdpClient())
                {
                    string message = $"CALL_ACCEPT|{UserName}|{LocalAudioPort}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udp.Send(data, data.Length, signalingPeer);
                    Debug.WriteLine($"[SEND] CALL_ACCEPT to {callerUsername}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi chấp nhận cuộc gọi: " + ex.Message);
            }
        }

        public void SendCallReject(IPEndPoint peer, string callerUsername)
        {
            try
            {
                IPEndPoint signalingPeer = new IPEndPoint(peer.Address, LAN_DISCOVERY_PORT);
                using (UdpClient udp = new UdpClient())
                {
                    string message = $"CALL_REJECT|{UserName}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udp.Send(data, data.Length, signalingPeer);
                    Debug.WriteLine($"[SEND] CALL_REJECT to {callerUsername}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi từ chối cuộc gọi: " + ex.Message);
            }
        }

        public void SendEndCall(IPEndPoint peer)
        {
            try
            {
                IPEndPoint signalingPeer = new IPEndPoint(peer.Address, LAN_DISCOVERY_PORT);
                using (UdpClient udp = new UdpClient())
                {
                    string message = $"END_CALL|{UserName}";
                    byte[] data = Encoding.UTF8.GetBytes(message);
                    udp.Send(data, data.Length, signalingPeer);
                    Debug.WriteLine($"[SEND] END_CALL");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi gửi tín hiệu kết thúc cuộc gọi: " + ex.Message);
            }
        }

        public void Stop()
        {
            try
            {
                udpListener?.Close();
                udpBroadcast?.Close();
            }
            catch { }
        }
    }

    public class CallRequestEventArgs : EventArgs
    {
        public string CallerUsername { get; set; }
        public string CallerDisplayName { get; set; }
        public string CallerGender { get; set; }
        public IPEndPoint CallerEndpoint { get; set; }
    }
}