using System.Diagnostics;
using System.Media;
using System.Net;

namespace AUDIO_NEW
{
    /// <summary>
    /// Qu?n l? logic cu?c g?i (incoming/outgoing)
    /// </summary>
    public class CallManager
    {
        private MainForm mainForm;
        private NetworkManager networkManager;
        private PeerManager peerManager;

        public AudioManager Audio { get; private set; }
        public string CurrentCallTarget { get; private set; }

        private SoundPlayer incomingRingPlayer;
        private SoundPlayer outgoingRingPlayer;
        private FormWait formWait;
        private callinterface currentCallForm;

        public CallManager(MainForm mainForm, NetworkManager networkManager, PeerManager peerManager)
        {
            this.mainForm = mainForm;
            this.networkManager = networkManager;
            this.peerManager = peerManager;

            // Subscribe to network events
            networkManager.CallRequestReceived += OnCallRequestReceived;
            networkManager.CallAccepted += OnCallAccepted;
            networkManager.CallRejected += OnCallRejected;
            networkManager.CallEnded += OnCallEnded;
        }

        /// <summary>
        /// B?t đ?u cu?c g?i đi (outgoing call)
        /// </summary>
        public void StartOutgoingCall(string targetUsername, string targetDisplayName, string targetGender, IPEndPoint peerEndpoint)
        {
            CurrentCallTarget = targetUsername;
            
            // ? FIX L?I #1 & #2: T?o Audio ? CallManager, dùng shared instance
            Audio = new AudioManager(networkManager.LocalAudioPort);
            Audio.SetPeer(peerEndpoint.Address.ToString(), peerEndpoint.Port);
            Audio.Start();  // ? START ngay, không cho peer accept

            currentCallForm = new callinterface(mainForm, CurrentCallTarget);
            currentCallForm.SetCallManager(this);
            currentCallForm.SetAudioFromCallManager(Audio);  // ? Pass audio t? CallManager

            // Play ringtone
            try
            {
                outgoingRingPlayer = new SoundPlayer(Properties.Resources.MusicOnHold);
                outgoingRingPlayer.PlayLooping();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CALL] Error playing ringtone: {ex.Message}");
            }

            // Send call request
            networkManager.SendCallRequest(targetUsername, peerEndpoint);

            // Show wait form (pass CallManager and peerEndpoint)
            formWait = new FormWait(targetDisplayName, targetGender, this, peerEndpoint);
            var result = formWait.ShowDialog();

            // Handle result
            if (formWait.IsCancelled)
            {
                Debug.WriteLine("[CALL] User cancelled from FormWait");
            }
            else if (formWait.IsAccepted)
            {
                currentCallForm.Show();
                Debug.WriteLine("[CALL] Call accepted, showing call interface");
            }
            else if (formWait.IsRejected)
            {
                Debug.WriteLine("[CALL] Call was rejected");
            }
        }

        /// <summary>
        /// H?y cu?c g?i đi (đư?c g?i t? FormWait khi user cancel)
        /// </summary>
        public void CancelOutgoingCall(IPEndPoint peerEndpoint)
        {
            outgoingRingPlayer?.Stop();
            outgoingRingPlayer?.Dispose();
            outgoingRingPlayer = null;

            networkManager.SendCallCancel(CurrentCallTarget, peerEndpoint);

            if (currentCallForm != null)
            {
                currentCallForm.Close();
                currentCallForm.Dispose();
                currentCallForm = null;
            }

            Audio?.StopAudio();
            Audio = null;

            CurrentCallTarget = "";

            Debug.WriteLine("[CALL] User cancelled the call");
        }

        private AgreeOrNot currentAgreeOrNotForm;

        /// <summary>
        /// X? l? khi nh?n đư?c yêu c?u g?i đ?n
        /// </summary>
        private void OnCallRequestReceived(object sender, CallRequestEventArgs e)
        {
            mainForm.Invoke(new Action(() =>
            {
                try
                {
                    incomingRingPlayer = new SoundPlayer(Properties.Resources.RingTone);
                    incomingRingPlayer.PlayLooping();
                }
                catch { }

                currentAgreeOrNotForm = new AgreeOrNot(mainForm, e.CallerDisplayName, e.CallerGender, e.CallerUsername);
                var result = currentAgreeOrNotForm.ShowDialog();

                incomingRingPlayer?.Stop();
                incomingRingPlayer?.Dispose();
                incomingRingPlayer = null;

                mainForm.Invoke(new Action(() =>
                {
                    if (result == DialogResult.OK)
                    {
                        AcceptIncomingCall(e.CallerUsername, e.CallerEndpoint);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        RejectIncomingCall(e.CallerUsername, e.CallerEndpoint);
                    }
                }));

                currentAgreeOrNotForm?.Dispose();
                currentAgreeOrNotForm = null;
            }));
        }

        private void AcceptIncomingCall(string callerUsername, IPEndPoint callerEndpoint)
        {
            currentCallForm = new callinterface(mainForm, callerUsername);
            currentCallForm.SetCallManager(this);
            // ? FIX L?I #2: Receiver START audio ngay (khác v?i caller wait)
            currentCallForm.StartAudioAsReceiver(callerEndpoint.Address.ToString(), callerEndpoint.Port, networkManager.LocalAudioPort);
            currentCallForm.Show();
            networkManager.SendCallAccept(callerEndpoint, callerUsername);
        }

        private void RejectIncomingCall(string callerUsername, IPEndPoint callerEndpoint)
        {
            networkManager.SendCallReject(callerEndpoint, callerUsername);
        }

        /// <summary>
        /// X? l? khi cu?c g?i đư?c ch?p nh?n
        /// </summary>
        private void OnCallAccepted(object sender, string acceptUser)
        {
            mainForm.Invoke(new Action(() =>
            {
                if (acceptUser == CurrentCallTarget)
                {
                    outgoingRingPlayer?.Stop();
                    outgoingRingPlayer?.Dispose();
                    outgoingRingPlayer = null;

                    if (formWait != null && !formWait.IsDisposed)
                    {
                        formWait.CallAccepted();
                    }

                    // ? FIX #3: Audio đ? đư?c Start() khi caller t?o
                    // Không c?n g?i g?, ch? c?n formWait close thôi
                    // Audio lu?ng receiver s? k?t n?i qua UDP
                }
            }));
        }

        /// <summary>
        /// X? l? khi cu?c g?i b? t? ch?i
        /// </summary>
        private void OnCallRejected(object sender, string rejectUser)
        {
            mainForm.Invoke(new Action(() =>
            {
                outgoingRingPlayer?.Stop();
                outgoingRingPlayer?.Dispose();
                outgoingRingPlayer = null;

                if (formWait != null && !formWait.IsDisposed)
                {
                    formWait.CallRejected();
                }

                Audio?.StopAudio();
                Audio = null;
                CurrentCallTarget = "";

                MessageBox.Show($"{rejectUser} Đã từ chối cuộc gọi!");
            }));
        }

        /// <summary>
        /// X? l? khi cu?c g?i k?t thúc
        /// </summary>
        private void OnCallEnded(object sender, string endUser)
        {
            mainForm.Invoke(new Action(() =>
            {
                Audio?.StopAudio();
                Audio = null;

                if (currentCallForm != null && currentCallForm.username == endUser)
                {
                    currentCallForm.Close();
                }

                MessageBox.Show($"Cu?c g?i v?i {endUser} đ? k?t thúc.");
            }));
        }

        /// <summary>
        /// K?t thúc cu?c g?i
        /// </summary>
        public void EndCall(string targetUsername)
        {
            var peerEndpoint = peerManager.GetPeerEndpoint(targetUsername);
            if (peerEndpoint != null)
            {
                networkManager.SendEndCall(peerEndpoint);
            }
        }

        public void Cleanup()
        {
            incomingRingPlayer?.Dispose();
            outgoingRingPlayer?.Dispose();
        }

        /// <summary>
        /// Get outgoing ring player (for callinterface to stop)
        /// </summary>
        public SoundPlayer GetOutgoingRingPlayer()
        {
            return outgoingRingPlayer;
        }

        /// <summary>
        /// Stop outgoing ring player
        /// </summary>
        public void StopOutgoingRing()
        {
            outgoingRingPlayer?.Stop();
            outgoingRingPlayer?.Dispose();
            outgoingRingPlayer = null;
        }
    }
}
