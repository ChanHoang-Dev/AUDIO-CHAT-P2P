namespace AUDIO_NEW
{
    public partial class StartForm : Form
    {
        Random random = new Random();

        int maxProgress = 200;
        int current = 0;
        string[] tips =
        {
                "Initializing audio engine...",
                "Preparing network modules...",
                "Setting up P2P channel...",
                "Loading voice codecs...",
                "Optimizing connection...",
                "Almost ready..."
            };
        int currentLine = 0;
        public StartForm()
        {
            InitializeComponent();
            this.Opacity = 0; // Fade-in bắt đầu từ 0
            timerFadeIn.Start();
            labelPercent.Parent = panelProgress; // parent chung
            labelPercent.BringToFront();
        }

        private void timerFadeIn_Tick(object sender, EventArgs e)
        {
            if (this.Opacity < 1)
                this.Opacity += 0.02;
            else
            {
                timerFadeIn.Stop();
                timerProgress.Start();
            }
        }

        private void timerProgress_Tick(object sender, EventArgs e)
        {
            int step = random.Next(1, 3); // tăng 2-5
            current += step;

            if (current > maxProgress) current = maxProgress;

            // Update %
            int percent = (current * 100) / maxProgress;
            labelPercent.Text = percent + "%";

            // Cập nhật thanh progress
            panelLoading.Width = (panelProgress.Width * percent) / 100;
            // Cập nhật labelLoading theo dòng
            if (currentLine < tips.Length)
            {
                // mỗi 20% progress chuyển dòng tiếp
                int threshold = ((currentLine + 1) * maxProgress) / tips.Length;
                if (current >= threshold)
                {
                    labelLoading.Text = tips[currentLine];
                    currentLine++;
                }
            }

            if (current >= maxProgress)
            {
                timerProgress.Stop();
                timerFadeOut.Start();
            }
        }

        private void timerFadeOut_Tick(object sender, EventArgs e)
        {
            this.Opacity -= 0.03;
            if (this.Opacity <= 0)
            {
                timerFadeOut.Stop();

                FormInfor fi = new FormInfor();
                fi.Show();
                this.Hide();
            }
        }

        private void labelTitle_Click(object sender, EventArgs e)
        {

        }
    }
}
