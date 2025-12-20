using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace AUDIO_NEW
{
    /// <summary>
    /// Form hiển thị khi đang chờ người nhận trả lời
    /// </summary>
    public partial class FormWait : Form
    {
        private string targetDisplayName;
        private string targetGender;
        private CallManager callManager;
        private IPEndPoint peerEndpoint;
        private Timer animationTimer;
        private int dotCount = 0;

        public bool IsAccepted { get; private set; } = false;
        public bool IsRejected { get; private set; } = false;
        public bool IsCancelled { get; private set; } = false;

        public FormWait(string displayName, string gender, CallManager callManager, IPEndPoint peerEndpoint)
        {
            this.targetDisplayName = displayName;
            this.targetGender = gender;
            this.callManager = callManager;
            this.peerEndpoint = peerEndpoint;

            InitializeComponent();

            // Set avatar based on gender
            if (picAvatar != null)
            {
                picAvatar.Image = UIHelper.GetAvatarByGender(targetGender, 150, 150);
            }

            if (picExit != null && Properties.Resources.ResourceManager.GetObject("Decline") != null)
            {
                picExit.Image = UIHelper.ResizeImage(
                    (Image)Properties.Resources.ResourceManager.GetObject("Decline"),
                    50, 50
                );
            }

            // Set and center labels
            if (lbDisplayName != null)
            {
                lbDisplayName.Text = targetDisplayName;
                lbDisplayName.Left = (this.ClientSize.Width - lbDisplayName.Width) / 2;
            }

            if (lbCalling != null)
            {
                lbCalling.Text = "Đang gọi";
                lbCalling.Left = (this.ClientSize.Width - lbCalling.Width) / 2;
            }

            StartAnimation();
        }

        private void StartAnimation()
        {
            animationTimer = new Timer();
            animationTimer.Interval = 500; // 0.5 giây
            animationTimer.Tick += timer1_Tick;
            animationTimer.Start();
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (lbCalling != null && !lbCalling.IsDisposed)
            {
                dotCount = (dotCount + 1) % 4;
                lbCalling.Text = "Đang gọi" + new string('.', dotCount);
                lbCalling.Left = (this.ClientSize.Width - lbCalling.Width) / 2;
            }
        }

        /// <summary>
        /// Được gọi từ CallManager khi nhận CALL_ACCEPT
        /// </summary>
        public void CallAccepted()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(CallAccepted));
                return;
            }

            animationTimer?.Stop();
            this.IsAccepted = true;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        /// <summary>
        /// Được gọi từ CallManager khi nhận CALL_REJECT
        /// </summary>
        public void CallRejected()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(CallRejected));
                return;
            }

            animationTimer?.Stop();
            this.IsRejected = true;
            MessageBox.Show($"{targetDisplayName} đã từ chối cuộc gọi.",
                "Cuộc gọi bị từ chối",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            animationTimer?.Stop();
            animationTimer?.Dispose();
            base.OnFormClosing(e);
        }

        /// <summary>
        /// User nhấn nút Cancel (X button)
        /// </summary>
        private void picExit_Click(object sender, EventArgs e)
        {
            animationTimer?.Stop();

            // ✅ GỬI CALL_CANCEL NGAY (CallManager sẽ gửi UDP message)
            callManager.CancelOutgoingCall(peerEndpoint);

            this.IsCancelled = true;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }
    }
}