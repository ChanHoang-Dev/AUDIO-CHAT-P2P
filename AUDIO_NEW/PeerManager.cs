using System.Diagnostics;
using System.Net;

namespace AUDIO_NEW
{
    /// <summary>
    /// Quản lý danh sách peers đã phát hiện
    /// </summary>
    public class PeerManager
    {
        private List<PeerInfo> discoveredPeers = new List<PeerInfo>();
        private Dictionary<string, IPEndPoint> peerMap = new Dictionary<string, IPEndPoint>();
        private System.Windows.Forms.Timer offlineCheckTimer;

        public event EventHandler<List<PeerInfo>> PeerListUpdated;

        public PeerManager()
        {
            // Timer để kiểm tra offline peers
            offlineCheckTimer = new System.Windows.Forms.Timer();
            offlineCheckTimer.Interval = 5000; // 5 giây
            offlineCheckTimer.Tick += (s, e) => CheckOfflinePeers();
            offlineCheckTimer.Start();
        }

        public void AddOrUpdatePeer(PeerInfo peerInfo)
        {
            lock (discoveredPeers)
            {
                lock (peerMap)
                {
                    peerMap[peerInfo.Username] = peerInfo.EndPoint;
                }

                var existing = discoveredPeers.FirstOrDefault(p => p.Username == peerInfo.Username);
                if (existing != null)
                {
                    existing.EndPoint = peerInfo.EndPoint;
                    existing.DisplayName = peerInfo.DisplayName;
                    existing.Gender = peerInfo.Gender;
                    existing.LastSeen = DateTime.Now;
                    Debug.WriteLine($"[PEER UPDATE] {peerInfo.Username} → {peerInfo.DisplayName}");
                }
                else
                {
                    discoveredPeers.Add(peerInfo);
                    Debug.WriteLine($"[PEER ADD] {peerInfo.Username} ({peerInfo.DisplayName})");
                }
            }

            PeerListUpdated?.Invoke(this, GetAllPeers());
        }

        public List<PeerInfo> GetAllPeers()
        {
            lock (discoveredPeers)
            {
                return new List<PeerInfo>(discoveredPeers);
            }
        }

        public PeerInfo GetPeer(string username)
        {
            lock (discoveredPeers)
            {
                return discoveredPeers.FirstOrDefault(p => p.Username == username);
            }
        }

        public IPEndPoint GetPeerEndpoint(string username)
        {
            lock (peerMap)
            {
                return peerMap.ContainsKey(username) ? peerMap[username] : null;
            }
        }

        public bool IsPeerOnline(string username)
        {
            lock (peerMap)
            {
                return peerMap.ContainsKey(username);
            }
        }

        private void CheckOfflinePeers()
        {
            lock (discoveredPeers)
            {
                var now = DateTime.Now;
                var offlinePeers = discoveredPeers
                    .Where(p => (now - p.LastSeen).TotalSeconds > 10)
                    .ToList();

                foreach (var offlinePeer in offlinePeers)
                {
                    discoveredPeers.Remove(offlinePeer);
                    lock (peerMap)
                    {
                        peerMap.Remove(offlinePeer.Username);
                    }
                    Debug.WriteLine($"[PEER OFFLINE] {offlinePeer.Username}");
                }

                if (offlinePeers.Count > 0)
                {
                    PeerListUpdated?.Invoke(this, GetAllPeers());
                }
            }
        }

        public void Stop()
        {
            offlineCheckTimer?.Stop();
            offlineCheckTimer?.Dispose();
        }
    }

    /// <summary>
    /// Thông tin về một peer
    /// </summary>
    public class PeerInfo
    {
        public string Username { get; set; }
        public string DisplayName { get; set; }
        public IPEndPoint EndPoint { get; set; }
        public string Gender { get; set; }
        public DateTime LastSeen { get; set; }
    }
}