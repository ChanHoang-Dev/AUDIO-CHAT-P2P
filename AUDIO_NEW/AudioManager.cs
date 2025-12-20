using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using NAudio.Wave;

namespace AUDIO_NEW
{
    public class AudioManager
    {
        private WaveInEvent waveIn;
        private WaveOutEvent waveOut;
        private BufferedWaveProvider waveProvider;
        private readonly WaveFormat audioFormat = new WaveFormat(8000, 16, 1); // 8kHz, 16-bit, mono

        private UdpClient udpSender;
        private UdpClient udpReceiver;
        private IPEndPoint remoteEP;

        private int localPort;
        private bool isRunning = false;

        public AudioManager(int localPort)
        {
            this.localPort = localPort;
            Debug.WriteLine($"[AUDIO] AudioManager created on port {localPort}");

            waveProvider = new BufferedWaveProvider(audioFormat)
            {
                BufferDuration = TimeSpan.FromSeconds(5), // ✅ FIX: 500s → 5s (hợp lý hơn)
                DiscardOnBufferOverflow = true
            };
        }

        /// <summary>
        /// Kết nối tới peer trên LAN
        /// </summary>
        public void SetPeer(string ip, int port)
        {
            remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
            Debug.WriteLine($"[AUDIO] SetPeer: {ip}:{port}");

            // ✅ FIX: Tạo sender ngay lập tức
            if (udpSender == null)
            {
                udpSender = new UdpClient();
                Debug.WriteLine("[AUDIO] UDP sender created");
            }
        }

        public void Start()
        {
            if (isRunning)
            {
                Debug.WriteLine("[AUDIO] Already running, skipping Start()");
                return;
            }

            Debug.WriteLine($"[AUDIO] Starting audio on port {localPort}");

            try
            {
                // ✅ FIX: Tạo receiver với proper error handling
                udpReceiver = new UdpClient();
                udpReceiver.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                udpReceiver.ExclusiveAddressUse = false;
                udpReceiver.Client.Bind(new IPEndPoint(IPAddress.Any, localPort));

                Debug.WriteLine($"[AUDIO] UDP receiver bound to port {localPort}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO ERROR] Failed to bind receiver: {ex.Message}");
                return;
            }

            isRunning = true;

            // Start receive loop
            Task.Run(() => ReceiveLoop());

            // Start playback
            try
            {
                waveOut = new WaveOutEvent();
                waveOut.Init(waveProvider);
                waveOut.Play();
                Debug.WriteLine("[AUDIO] Playback started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO ERROR] Failed to start playback: {ex.Message}");
            }

            // Start recording
            try
            {
                waveIn = new WaveInEvent
                {
                    WaveFormat = audioFormat,
                    BufferMilliseconds = 20
                };
                waveIn.DataAvailable += OnAudioDataAvailable;
                waveIn.StartRecording();
                Debug.WriteLine("[AUDIO] Recording started");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO ERROR] Failed to start recording: {ex.Message}");
            }
        }

        /// <summary>
        /// ✅ FIX LỖI #1: Thêm StartAudioStream() method
        /// Được gọi từ CallManager sau khi nhận CALL_ACCEPT
        /// </summary>
        public void StartAudioStream()
        {
            if (isRunning)
            {
                Debug.WriteLine("[AUDIO] Audio stream already running");
                return;
            }
            
            Start();
            Debug.WriteLine("[AUDIO] Audio stream started");
        }

        private void OnAudioDataAvailable(object sender, WaveInEventArgs e)
        {
            if (!isRunning || udpSender == null || remoteEP == null) return;

            try
            {
                byte[] pcmuData = EncodePCMU(e.Buffer, e.BytesRecorded);
                udpSender.Send(pcmuData, pcmuData.Length, remoteEP);

                // ✅ DEBUG: Log first few packets
                if (DateTime.Now.Second % 5 == 0) // Log mỗi 5 giây
                {
                    Debug.WriteLine($"[AUDIO] Sent {pcmuData.Length} bytes to {remoteEP}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO ERROR] Send error: {ex.Message}");
            }
        }

        private void ReceiveLoop()
        {
            Debug.WriteLine("[AUDIO] Receive loop started");
            int packetCount = 0;

            while (isRunning)
            {
                try
                {
                    var ep = new IPEndPoint(IPAddress.Any, 0);
                    var data = udpReceiver.Receive(ref ep);

                    if (data != null && data.Length > 0)
                    {
                        packetCount++;

                        //  DEBUG: Log first few packets
                        if (packetCount <= 5)
                        {
                            Debug.WriteLine($"[AUDIO] Received packet #{packetCount}: {data.Length} bytes from {ep}");
                        }

                        var pcmData = DecodePCMU(data);
                        waveProvider.AddSamples(pcmData, 0, pcmData.Length);
                    }
                }
                catch (ObjectDisposedException)
                {
                    Debug.WriteLine("[AUDIO] Receiver disposed, exiting loop");
                    break;
                }
                catch (SocketException sex)
                {
                    Debug.WriteLine($"[AUDIO] Socket error: {sex.Message}");
                    break;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[AUDIO ERROR] Receive error: {ex.Message}");
                }
            }

            Debug.WriteLine($"[AUDIO] Receive loop ended. Total packets: {packetCount}");
        }

        public void StopAudio()
        {
            Debug.WriteLine("[AUDIO] Stopping audio");
            isRunning = false;

            try
            {
                waveIn?.StopRecording();
                waveIn?.Dispose();
                waveIn = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO] Error stopping waveIn: {ex.Message}");
            }

            try
            {
                waveOut?.Stop();
                waveOut?.Dispose();
                waveOut = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[AUDIO] Error stopping waveOut: {ex.Message}");
            }

            try { udpReceiver?.Close(); } catch { }
            try { udpSender?.Close(); } catch { }

            udpReceiver = null;
            udpSender = null;

            Debug.WriteLine("[AUDIO] Audio stopped");
        }

        #region PCMU Codec

        private byte[] EncodePCMU(byte[] pcmData, int length)
        {
            int samples = length / 2;
            var encoded = new byte[samples];
            for (int i = 0; i < samples; i++)
            {
                short sample = BitConverter.ToInt16(pcmData, i * 2);
                encoded[i] = LinearToPCMU(sample);
            }
            return encoded;
        }

        private byte[] DecodePCMU(byte[] pcmuData)
        {
            var decoded = new byte[pcmuData.Length * 2];
            for (int i = 0; i < pcmuData.Length; i++)
            {
                short sample = PCMUToLinear(pcmuData[i]);
                Buffer.BlockCopy(BitConverter.GetBytes(sample), 0, decoded, i * 2, 2);
            }
            return decoded;
        }

        private static byte LinearToPCMU(short sample)
        {
            const int Bias = 0x84;
            const int Clip = 32635;

            int sign = (sample >> 8) & 0x80;
            if (sign != 0) sample = (short)-sample;
            if (sample > Clip) sample = Clip;
            sample = (short)(sample + Bias);

            int exponent = 7;
            for (int expMask = 0x4000; (sample & expMask) == 0 && exponent > 0; exponent--, expMask >>= 1) ;
            int mantissa = (sample >> (exponent + 3)) & 0x0F;
            int companded = ~(sign | (exponent << 4) | mantissa);

            return (byte)companded;
        }

        private static short PCMUToLinear(byte ulaw)
        {
            ulaw = (byte)~ulaw;
            int sign = ulaw & 0x80;
            int exponent = (ulaw >> 4) & 0x07;
            int mantissa = ulaw & 0x0F;

            int sample = ((mantissa << 3) + 0x84) << exponent;
            sample -= 0x84;

            return sign != 0 ? (short)-sample : (short)sample;
        }

        #endregion
    }
}