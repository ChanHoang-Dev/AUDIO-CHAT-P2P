namespace AUDIO_NEW
{
    partial class AgreeOrNot
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
            picAvatar = new PictureBox();
            lbDisplayName = new Label();
            picAgree = new PictureBox();
            picNot = new PictureBox();
            ((System.ComponentModel.ISupportInitialize)picAvatar).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAgree).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picNot).BeginInit();
            SuspendLayout();
            // 
            // picAvatar
            // 
            picAvatar.Location = new Point(110, 66);
            picAvatar.Name = "picAvatar";
            picAvatar.Size = new Size(150, 150);
            picAvatar.TabIndex = 0;
            picAvatar.TabStop = false;
            // 
            // lbDisplayName
            // 
            lbDisplayName.AutoSize = true;
            lbDisplayName.Location = new Point(166, 241);
            lbDisplayName.Name = "lbDisplayName";
            lbDisplayName.Size = new Size(38, 15);
            lbDisplayName.TabIndex = 1;
            lbDisplayName.Text = "label1";
            // 
            // picAgree
            // 
            picAgree.Location = new Point(74, 421);
            picAgree.Name = "picAgree";
            picAgree.Size = new Size(50, 50);
            picAgree.TabIndex = 2;
            picAgree.TabStop = false;
            picAgree.Click += picAgree_Click;
            // 
            // picNot
            // 
            picNot.Location = new Point(279, 421);
            picNot.Name = "picNot";
            picNot.Size = new Size(50, 50);
            picNot.TabIndex = 3;
            picNot.TabStop = false;
            picNot.Click += picNot_Click;
            // 
            // AgreeOrNot
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.MistyRose;
            ClientSize = new Size(400, 576);
            Controls.Add(picNot);
            Controls.Add(picAgree);
            Controls.Add(lbDisplayName);
            Controls.Add(picAvatar);
            FormBorderStyle = FormBorderStyle.None;
            Name = "AgreeOrNot";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "AgreeOrNot";
            ((System.ComponentModel.ISupportInitialize)picAvatar).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAgree).EndInit();
            ((System.ComponentModel.ISupportInitialize)picNot).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox picAvatar;
        private Label lbDisplayName;
        private PictureBox picAgree;
        private PictureBox picNot;
    }
}