using System;
using System.Drawing;
using System.Windows.Forms;

namespace AUDIO_NEW
{
    public partial class AgreeOrNot : Form
    {
        // Events
        public event Action Accepted;
        public event Action Rejected;

        // Properties
        public bool IsAccepted { get; private set; } = false;
        public bool IsRejected { get; private set; } = false;

        private MainForm mainForm;
        private string callerUsername;
        private string callerDisplayName;
        private string callerGender;

        public Image ResizeImage(Image img, int width, int height)
        {
            return UIHelper.ResizeImage(img, width, height);
        }

        // Constructor cơ bản
        public AgreeOrNot(string callerDisplayName, string callerGender)
        {
            InitializeComponent();
            this.callerDisplayName = callerDisplayName;
            this.callerGender = callerGender;

            InitializeUI();
            ShowCallerInfo();
        }

        // Constructor với MainForm
        public AgreeOrNot(MainForm mainForm, string callerDisplayName, string callerGender)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.callerDisplayName = callerDisplayName;
            this.callerGender = callerGender;

            InitializeUI();
            ShowCallerInfo();
        }

        // ✅ Constructor với username (cho CallManager)
        public AgreeOrNot(MainForm mainForm, string callerDisplayName, string callerGender, string callerUsername)
        {
            InitializeComponent();
            this.mainForm = mainForm;
            this.callerDisplayName = callerDisplayName;
            this.callerGender = callerGender;
            this.callerUsername = callerUsername;

            InitializeUI();
            ShowCallerInfo();
        }

        private void InitializeUI()
        {
            if (picAgree != null && Properties.Resources.ResourceManager.GetObject("Call") != null)
            {
                picAgree.Image = ResizeImage(
                    (Image)Properties.Resources.ResourceManager.GetObject("Call"),
                    50, 50
                );
            }

            if (picNot != null && Properties.Resources.ResourceManager.GetObject("Decline") != null)
            {
                picNot.Image = ResizeImage(
                    (Image)Properties.Resources.ResourceManager.GetObject("Decline"),
                    50, 50
                );
            }
        }

        private void ShowCallerInfo()
        {
            if (lbDisplayName != null)
            {
                lbDisplayName.Text = callerDisplayName;
            }

            if (picAvatar != null)
            {
                picAvatar.Image = UIHelper.GetAvatarByGender(callerGender, 150, 150);
            }
        }

        private void picAgree_Click(object sender, EventArgs e)
        {
            IsAccepted = true;
            IsRejected = false;

            // Fire event nếu có subscriber
            Accepted?.Invoke();

            // Set DialogResult
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void picNot_Click(object sender, EventArgs e)
        {
            IsAccepted = false;
            IsRejected = true;

            // Fire event nếu có subscriber
            Rejected?.Invoke();

            // Set DialogResult
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        /// <summary>
        /// ✅ Method được gọi khi caller cancel cuộc gọi
        /// </summary>
        public void CallCancelled()
        {
            if (this.InvokeRequired)
            {
                this.Invoke(new Action(CallCancelled));
                return;
            }

            // Đóng form với DialogResult.Abort
            // (khác với Cancel = user từ chối)
            this.DialogResult = DialogResult.Abort;
            this.Close();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Nếu user đóng form bằng X button
            if (e.CloseReason == CloseReason.UserClosing &&
                !IsAccepted &&
                !IsRejected &&
                this.DialogResult != DialogResult.Abort)
            {
                // Coi như từ chối
                IsRejected = true;
                this.DialogResult = DialogResult.Cancel;

                // Fire Rejected event
                Rejected?.Invoke();
            }
        }
    }
}