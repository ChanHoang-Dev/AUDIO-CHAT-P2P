using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Diagnostics;

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
        private Label lbFileName;
        private VoiceMessageManager voiceManager;

        public VoiceMessageBubble(VoiceMessage voiceMessage)
        {
            this.voiceMessage = voiceMessage;
            string baseFolder = GetBaseFolder();
            this.voiceManager = new VoiceMessageManager(baseFolder);
            InitializeComponents();
            LoadContent();
        }

        private void InitializeComponents()
        {
            this.Size = new Size(400, 180);
            this.BackColor = voiceMessage.IsOwn ? Color.LightBlue : Color.LightGray;
            this.BorderStyle = BorderStyle.FixedSingle;
            this.Margin = new Padding(5);

            lbSender = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 10, FontStyle.Bold),
                Location = new Point(10, 10),
                Text = voiceMessage.SenderName
            };
            this.Controls.Add(lbSender);

            lbDuration = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 9, FontStyle.Regular),
                Location = new Point(10, 35),
                ForeColor = Color.DarkGray,
                Text = $"Th?i gian: {voiceMessage.Duration}s"
            };
            this.Controls.Add(lbDuration);

            lbFileName = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Italic),
                Location = new Point(10, 50),
                ForeColor = Color.DarkGray,
                Text = $"File: {voiceMessage.AudioFileName}"
            };
            this.Controls.Add(lbFileName);

            btnPlay = new Button
            {
                Text = "? Phát",
                Location = new Point(10, 70),
                Size = new Size(80, 35),
                BackColor = Color.LightSeaGreen,
                ForeColor = Color.White,
                Font = new Font("Arial", 9, FontStyle.Bold)
            };
            btnPlay.Click += BtnPlay_Click;
            this.Controls.Add(btnPlay);

            Label lbTranscriptionTitle = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 8, FontStyle.Bold),
                Location = new Point(100, 70),
                Text = "Chuy?n ð?i vãn b?n:"
            };
            this.Controls.Add(lbTranscriptionTitle);

            lbTranscription = new Label
            {
                AutoSize = false,
                Font = new Font("Arial", 8, FontStyle.Regular),
                Location = new Point(100, 85),
                Size = new Size(285, 65),
                Text = "[Ðang t?i n?i dung...]",
                BackColor = Color.White,
                BorderStyle = BorderStyle.FixedSingle,
                Padding = new Padding(5)
            };
            this.Controls.Add(lbTranscription);

            lbTimestamp = new Label
            {
                AutoSize = true,
                Font = new Font("Arial", 7, FontStyle.Italic),
                Location = new Point(10, 160),
                ForeColor = Color.Gray,
                Text = voiceMessage.Timestamp.ToString("HH:mm:ss")
            };
            this.Controls.Add(lbTimestamp);
        }

        private void LoadContent()
        {
            try
            {
                string baseFolder = GetBaseFolder();
                string transcriptionPath = Path.Combine(baseFolder, "voice_messages", voiceMessage.TranscriptionFileName);
                
                Debug.WriteLine($"[VOICE UI] Loading transcription from: {transcriptionPath}");
                
                if (File.Exists(transcriptionPath))
                {
                    string content = File.ReadAllText(transcriptionPath, Encoding.UTF8);
                    lbTranscription.Text = string.IsNullOrWhiteSpace(content) 
                        ? "[Chýa có chuy?n ð?i]" 
                        : content;
                    Debug.WriteLine($"[VOICE UI] Transcription loaded: {content.Substring(0, Math.Min(50, content.Length))}...");
                }
                else
                {
                    lbTranscription.Text = $"[File không t?m th?y: {voiceMessage.TranscriptionFileName}]";
                    Debug.WriteLine($"[VOICE UI] Transcription file not found: {transcriptionPath}");
                }
            }
            catch (Exception ex)
            {
                lbTranscription.Text = $"[L?i: {ex.Message}]";
                Debug.WriteLine($"[VOICE UI] Error loading transcription: {ex.Message}");
            }
        }

        private string GetBaseFolder()
        {
            // S? d?ng UserDataPath t? VoiceMessage n?u có, n?u không dùng hardcode
            if (!string.IsNullOrEmpty(voiceMessage?.UserDataPath))
            {
                return voiceMessage.UserDataPath;
            }

            // Fallback: hardcode path
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "AUDIO_NEW",
                "user_data"
            );
            return appDataPath;
        }

        private void BtnPlay_Click(object sender, EventArgs e)
        {
            try
            {
                string baseFolder = GetBaseFolder();
                string audioPath = Path.Combine(baseFolder, "voice_messages", voiceMessage.AudioFileName);
                
                Debug.WriteLine($"[VOICE UI] Attempting to play: {audioPath}");
                
                if (!File.Exists(audioPath))
                {
                    MessageBox.Show($"File không t?m th?y: {audioPath}", "L?i");
                    Debug.WriteLine($"[VOICE UI] Audio file not found: {audioPath}");
                    return;
                }

                voiceManager.PlayAudio(audioPath);
                Debug.WriteLine($"[VOICE UI] Playing: {audioPath}");
                MessageBox.Show($"Ðang phát: {voiceMessage.AudioFileName}", "Thông báo");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i phát âm thanh: {ex.Message}", "L?i");
                Debug.WriteLine($"[VOICE UI] Play error: {ex.Message}");
            }
        }
    }
}
