
using System;
using System.Drawing;
using System.Windows.Forms;

namespace AUDIO_NEW
{
    /// <summary>
    /// Form interface cu?c g?i - Hi?n th? khi đang trong cu?c g?i
    /// </summary>
    public partial class callinterface : Form
    {
        // Dependencies
        private MainForm mainForm;
        private CallManager callManager;

        // Audio
        public AudioManager audio;

        // Peer info
        public string username;
        private string peerIP;
        private int peerPort;

        public callinterface(MainForm mainForm, string username)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.username = username;
            pictureBox1.Image = UIHelper.ResizeImage(Properties.Resources.Decline, pictureBox1.Width, pictureBox1.Height);
        }

        /// <summary>
        /// Set reference to CallManager (đư?c g?i t? MainForm)
        /// </summary>
        public void SetCallManager(CallManager callManager)
        {
            this.callManager = callManager;
        }

        /// <summary>
        /// Set reference audio t? CallManager (FIX L?I #1)
        /// </summary>
        public void SetAudioFromCallManager(AudioManager audioManager)
        {
            this.audio = audioManager;
        }

        #region Audio Management

        /// <summary>
        /// Kh?i t?o audio cho ngư?i G?I (caller/initiator)
        /// ? UNUSED - CallManager s? pass audio thay v? t?o ? đây
        /// </summary>
        public void StartAudioAsCaller(string peerIP, int peerPort, int localPort, bool isInitial = false)
        {
            this.peerIP = peerIP;
            this.peerPort = peerPort;
            // Deprecated: Audio now set by CallManager via SetAudioFromCallManager
        }

        /// <summary>
        /// Start audio stream (g?i sau khi nh?n CALL_ACCEPT)
        /// </summary>
        public void StartAudioStream()
        {
            audio?.Start();
        }

        /// <summary>
        /// Kh?i t?o audio cho ngư?i NH?N (receiver)
        /// ? Receiver t?o audio m?i v? CallManager không t?o cho receiver
        /// </summary>
        public void StartAudioAsReceiver(string peerIP, int peerPort, int localPort)
        {
            this.peerIP = peerIP;
            this.peerPort = peerPort;

            audio = new AudioManager(localPort);
            audio.SetPeer(peerIP, peerPort);
            audio.Start(); // Receiver start ngay
        }

        /// <summary>
        /// D?ng audio stream
        /// </summary>
        public void StopAudio()
        {
            audio?.StopAudio();
            audio = null;
        }

        #endregion

        #region UI Events

        /// <summary>
        /// X? l? khi user nh?n nút Stop/Decline
        /// </summary>
        //private void btnStopVoice_Click(object sender, EventArgs e)
        //{
        //    EndCall();
        //}

        /// <summary>
        /// K?t thúc cu?c g?i
        /// </summary>
        private void EndCall()
        {
            // D?ng audio
            StopAudio();

            // D?ng nh?c chuông ch? (n?u có)
            if (callManager != null)
            {
                callManager.StopOutgoingRing();
            }

            // G?i tín hi?u END_CALL
            mainForm.SendEndCall(username);

            // Đóng form
            mainForm.CloseCallInterface(this);
        }

        #endregion

        #region Form Events

        /// <summary>
        /// X? l? khi form đóng
        /// </summary>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Cleanup audio n?u c?n
            if (audio != null)
            {
                StopAudio();
            }
        }

        #endregion

        #region UI Helpers

        /// <summary>
        /// Resize image v?i ch?t lư?ng cao
        /// </summary>
        public Image ResizeImage(Image img, int width, int height)
        {
            return UIHelper.ResizeImage(img, width, height);
        }

        #endregion

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            EndCall();
        }
    }
}
