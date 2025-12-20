namespace AUDIO_NEW
{
    partial class FormWait
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
            picAvatar = new PictureBox();
            lbDisplayName = new Label();
            lbCalling = new Label();
            picExit = new PictureBox();
            timer1 = new System.Windows.Forms.Timer(components);
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picExit).BeginInit();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new Point(119, 46);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(150, 150);
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            // 
            // lbDisplayName
            // 
            lbDisplayName.AutoSize = true;
            lbDisplayName.Location = new Point(170, 228);
            lbDisplayName.Name = "lbDisplayName";
            lbDisplayName.Size = new Size(38, 15);
            lbDisplayName.TabIndex = 1;
            lbDisplayName.Text = "label1";
            // 
            // lbCalling
            // 
            lbCalling.AutoSize = true;
            lbCalling.Font = new Font("Times New Roman", 21.75F, FontStyle.Bold, GraphicsUnit.Point, 163);
            lbCalling.Location = new Point(148, 272);
            lbCalling.Name = "lbCalling";
            lbCalling.Size = new Size(88, 32);
            lbCalling.TabIndex = 2;
            lbCalling.Text = "label1";
            // 
            // picExit
            // 
            picExit.Location = new Point(170, 342);
            picExit.Name = "picExit";
            picExit.Size = new Size(50, 50);
            picExit.TabIndex = 3;
            picExit.TabStop = false;
            picExit.Click += picExit_Click;
            // 
            // timer1
            // 
            timer1.Interval = 500;
            timer1.Tick += timer1_Tick;
            // 
            // FormWait
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.RosyBrown;
            ClientSize = new Size(405, 431);
            Controls.Add(picExit);
            Controls.Add(lbCalling);
            Controls.Add(lbDisplayName);
            Controls.Add(picAvatar);
            FormBorderStyle = FormBorderStyle.None;
            Name = "FormWait";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "FormWait";
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ((System.ComponentModel.ISupportInitialize)picExit).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Label lbDisplayName;
        private Label lbCalling;
        private PictureBox picExit;
        private System.Windows.Forms.Timer timer1;
    }
}