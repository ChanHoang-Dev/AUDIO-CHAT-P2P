using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace AUDIO_NEW
{
    public partial class SaveVoice : Form
    {
        private TimeSpan elapsedTime = TimeSpan.Zero;
        private VoiceMessageManager voiceManager;
        private MainForm mainForm;
        private string currentRecordingPath;
        private bool isRecording = false;

        public SaveVoice(MainForm mainForm, string userDataPath)
        {
            InitializeComponent();
            
            this.mainForm = mainForm;
            voiceManager = new VoiceMessageManager(userDataPath);
            
            // Load images
            picSendVoice.Image = UIHelper.ResizeImage(Properties.Resources.SendVoice, 50, 50);
            picStartVoice.Image = UIHelper.ResizeImage(Properties.Resources.StartVoice, 50, 50);
            picStopVoice.Image = UIHelper.ResizeImage(Properties.Resources.StopVoice, 50, 50);
            lbTime.Text = "00:00";

            // Setup timer
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
        }

        #region Recording Control

        private void picStartVoice_Click(object sender, EventArgs e)
        {
            if (isRecording)
            {
                MessageBox.Show("Ðang ghi âm r?i!");
                return;
            }

            try
            {
                timer1.Stop(); // D?ng timer c? trý?c khi b?t ð?u
                voiceManager.StartRecording();
                isRecording = true;
                elapsedTime = TimeSpan.Zero;
                lbTime.Text = "00:00";
                lbInfor.Text = "Ðang ghi âm...";
                timer1.Start();

                picStartVoice.Enabled = false;
                picStopVoice.Enabled = true;
                picSaveInDB.Enabled = false;
                picSendVoice.Enabled = false;

                Debug.WriteLine("[VOICE UI] Recording started");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi b?t ð?u ghi âm: {ex.Message}");
            }
        }

        private void picStopVoice_Click(object sender, EventArgs e)
        {
            if (!isRecording)
            {
                MessageBox.Show("Chýa b?t ð?u ghi âm!");
                return;
            }

            try
            {
                currentRecordingPath = voiceManager.StopRecording();
                isRecording = false;
                timer1.Stop();
                lbInfor.Text = "Ð? d?ng ghi âm.";

                picStartVoice.Enabled = true;
                picStopVoice.Enabled = false;
                picSaveInDB.Enabled = true;
                picSendVoice.Enabled = true;

                Debug.WriteLine($"[VOICE UI] Recording stopped: {currentRecordingPath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi d?ng ghi âm: {ex.Message}");
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (isRecording)
            {
                elapsedTime = elapsedTime.Add(TimeSpan.FromSeconds(1));
                lbTime.Text = elapsedTime.ToString(@"mm\:ss");
            }
        }

        #endregion

        #region Save to Database

        private async void picSaveInDB_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRecordingPath))
            {
                MessageBox.Show("Chýa có b?n ghi âm nào!");
                return;
            }

            try
            {
                lbInfor.Text = "Ðang lýu...";
                picSaveInDB.Enabled = false;

                // L?y thông tin ngý?i g?i t? mainForm
                string senderId = ""; // TODO: Get from mainForm.networkManager.UserName
                string senderName = ""; // TODO: Get from mainForm.userDataManager.DisplayName
                string recipientId = ""; // TODO: Get from mainForm.currentCallTarget

                // Bý?c 1: Speech-to-Text
                lbInfor.Text = "Ðang chuy?n ð?i speech to text...";
                string transcription = await voiceManager.TranscribeAudioAsync(currentRecordingPath);

                // Bý?c 2: Lýu voice message
                var voiceMessage = await voiceManager.SaveVoiceMessage(
                    senderId: senderId,
                    senderName: senderName,
                    recipientId: recipientId,
                    durationSeconds: (int)elapsedTime.TotalSeconds,
                    audioFilePath: currentRecordingPath,
                    transcription: transcription
                );

                if (voiceMessage != null)
                {
                    lbInfor.Text = "? Ð? lýu vào cõ s? d? li?u";
                    MessageBox.Show("Lýu thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reset form
                    ResetForm();
                }
                else
                {
                    lbInfor.Text = "? L?i khi lýu";
                    MessageBox.Show("L?i khi lýu voice message!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lbInfor.Text = "? L?i khi lýu";
                MessageBox.Show($"L?i: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                picSaveInDB.Enabled = true;
            }
        }

        #endregion

        #region Send Voice Message

        private async void picSendVoice_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(currentRecordingPath))
            {
                MessageBox.Show("Chýa có b?n ghi âm nào!");
                return;
            }

            try
            {
                lbInfor.Text = "Ðang g?i...";
                picSendVoice.Enabled = false;

                // L?y thông tin ngý?i g?i t? mainForm
                string senderId = mainForm.GetUserName(); // Get from mainForm
                string senderName = mainForm.GetDisplayName(); // Get from mainForm
                string recipientId = mainForm.GetCurrentCallTarget(); // Get current chat target

                // Speech-to-Text
                lbInfor.Text = "Ðang chuy?n ð?i speech to text...";
                string transcription = await voiceManager.TranscribeAudioAsync(currentRecordingPath);

                // Lýu voice message
                var voiceMessage = await voiceManager.SaveVoiceMessage(
                    senderId: senderId,
                    senderName: senderName,
                    recipientId: recipientId,
                    durationSeconds: (int)elapsedTime.TotalSeconds,
                    audioFilePath: currentRecordingPath,
                    transcription: transcription
                );

                if (voiceMessage != null)
                {
                    voiceMessage.IsOwn = true;

                    // G?i voice message qua network
                    mainForm.SendVoiceMessage(voiceMessage);

                    lbInfor.Text = "? Ð? g?i";
                    MessageBox.Show("G?i thành công!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    ResetForm();
                    this.Close(); // Close form after sending
                }
                else
                {
                    lbInfor.Text = "? L?i khi g?i";
                    MessageBox.Show("L?i khi lýu voice message!", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                lbInfor.Text = "? L?i khi g?i";
                MessageBox.Show($"L?i: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                picSendVoice.Enabled = true;
            }
        }

        #endregion

        #region Cancel Recording

        private void picCancelVoice_Click(object sender, EventArgs e)
        {
            try
            {
                if (isRecording)
                {
                    voiceManager.StopRecording();
                    isRecording = false;
                    timer1.Stop();
                }

                ResetForm();
                MessageBox.Show("Ð? h?y ghi âm.", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        #endregion

        #region Helper Methods

        private void ResetForm()
        {
            elapsedTime = TimeSpan.Zero;
            currentRecordingPath = null;
            lbTime.Text = "00:00";
            lbInfor.Text = "";

            picStartVoice.Enabled = true;
            picStopVoice.Enabled = false;
            picSaveInDB.Enabled = false;
            picSendVoice.Enabled = false;
        }

        private void CenterToScreen()
        {
            lbTime.Left = (this.ClientSize.Width - lbTime.Width) / 2;
            lbTime.Top = (this.ClientSize.Height - lbTime.Height) / 2;
        }

        #endregion

        #region Form Events

        private void SaveVoice_Load(object sender, EventArgs e)
        {
            CenterToScreen();
            lbInfor.Text = "";
            ResetForm();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            
            if (isRecording)
            {
                voiceManager.StopRecording();
            }
            voiceManager.Cleanup();
        }

        #endregion
    }
}
