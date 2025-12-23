namespace AUDIO_NEW
{
    partial class SaveVoice
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            picStartVoice = new PictureBox();
            lbTime = new Label();
            picSendVoice = new PictureBox();
            picStopVoice = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            lbInfor = new Label();
            ((System.ComponentModel.ISupportInitialize)picStartVoice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSendVoice).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picStopVoice).BeginInit();
            SuspendLayout();
            // 
            // picStartVoice
            // 
            picStartVoice.Location = new Point(43, 223);
            picStartVoice.Name = "picStartVoice";
            picStartVoice.Size = new Size(50, 50);
            picStartVoice.TabIndex = 0;
            picStartVoice.TabStop = false;
            picStartVoice.Click += picStartVoice_Click;
            // 
            // lbTime
            // 
            lbTime.AutoSize = true;
            lbTime.Font = new Font("Times New Roman", 27.75F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lbTime.Location = new Point(186, 109);
            lbTime.Name = "lbTime";
            lbTime.Size = new Size(113, 42);
            lbTime.TabIndex = 1;
            lbTime.Text = "label1";
            // 
            // picSendVoice
            // 
            picSendVoice.Location = new Point(428, 223);
            picSendVoice.Name = "picSendVoice";
            picSendVoice.Size = new Size(50, 50);
            picSendVoice.TabIndex = 3;
            picSendVoice.TabStop = false;
            picSendVoice.Click += picSendVoice_Click;
            // 
            // picStopVoice
            // 
            picStopVoice.Location = new Point(233, 223);
            picStopVoice.Name = "picStopVoice";
            picStopVoice.Size = new Size(50, 50);
            picStopVoice.TabIndex = 4;
            picStopVoice.TabStop = false;
            picStopVoice.Click += picStopVoice_Click;
            // 
            // timer1
            // 
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // lbInfor
            // 
            lbInfor.AutoSize = true;
            lbInfor.Font = new Font("Times New Roman", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 0);
            lbInfor.Location = new Point(166, 42);
            lbInfor.Name = "lbInfor";
            lbInfor.Size = new Size(63, 24);
            lbInfor.TabIndex = 5;
            lbInfor.Text = "label1";
            lbInfor.TextAlign = ContentAlignment.MiddleCenter;
            // 
            // SaveVoice
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(528, 328);
            Controls.Add(lbInfor);
            Controls.Add(picStopVoice);
            Controls.Add(picSendVoice);
            Controls.Add(lbTime);
            Controls.Add(picStartVoice);
            FormBorderStyle = FormBorderStyle.None;
            Name = "SaveVoice";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SaveVoice";
            Load += SaveVoice_Load;
            ((System.ComponentModel.ISupportInitialize)picStartVoice).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSendVoice).EndInit();
            ((System.ComponentModel.ISupportInitialize)picStopVoice).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picStartVoice;
        private Label lbTime;
        private PictureBox picSendVoice;
        private PictureBox picStopVoice;
        private System.Windows.Forms.Timer timer1;
        private Label lbInfor;
    }
}
