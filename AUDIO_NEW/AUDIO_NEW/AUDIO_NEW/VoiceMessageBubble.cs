using System;
using System.Drawing;
using System.Windows.Forms;

namespace AUDIO_NEW
{
    /// <summary>
    /// Custom control ð? hi?n th? voice message trong FlowLayoutPanel
    /// </summary>
    public class VoiceMessageBubble : UserControl
    {
        private VoiceMessage voiceMessage;
        private Label lbSender;
        private Label lbDuration;
        private Label lbTranscription;
        private Button btnPlay;
        private Label lbTimestamp;

        public VoiceMessageBubble(VoiceMessage voiceMessage)
        {
            this.voiceMessage = voiceMessage;
            InitializeComponents();
            SetContent();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(350, 150);
            this.BackColor = voiceMessage.IsOwn ? Color.LightBlue : Color.LightGray;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);

            // Sender name
            lbSender = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                Text = voiceMessage.SenderName
            };
            this.Controls.Add(lbSender);

            // Duration label
            lbDuration = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Regular),
                Location = new Point(10, 35),
                ForeColor = Color.DarkGray,
                Text = $"Th?i gian: {voiceMessage.Duration}s"
            };
            this.Controls.Add(lbDuration);

            // Play button
            btnPlay = new Button
            {
                Text = "? Phát",
                Location = new Point(10, 55),
                Size = new Size(70, 30),
                BackColor = Color.LightSeaGreen,
                ForeColor = Color.White
            };
            btnPlay.Click += BtnPlay_Click;
            this.Controls.Add(btnPlay);

            // Transcription
            lbTranscription = new Label
            {
                AutoSize = false,
                Font = new Font("Arial", 9, FontStyle.Regular),
                Location = new Point(90, 55),
                Size = new Size(245, 65),
                Text = voiceMessage.TranscriptionFileName,
                Visible = true
            };
            this.Controls.Add(lbTranscription);

            // Timestamp
            lbTimestamp = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                Location = new Point(10, 130),
                ForeColor = Color.Gray,
                Text = voiceMessage.Timestamp.ToString("HH:mm:ss")
            };
            this.Controls.Add(lbTimestamp);
        }

        private void SetContent()
        {
            // TODO: L?y n?i dung transcription t? file .txt
            lbTranscription.Text = "[N?i dung ghi âm]"; // Placeholder
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                // TODO: Phát file .wav
                MessageBox.Show($"Ðang phát: {voiceMessage.AudioFileName}", "Thông báo");
                System.Diagnostics.Debug.WriteLine($"[VOICE UI] Playing: {voiceMessage.AudioFileName}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i phát âm thanh: {ex.Message}", "L?i");
            }
        }
    }
}
