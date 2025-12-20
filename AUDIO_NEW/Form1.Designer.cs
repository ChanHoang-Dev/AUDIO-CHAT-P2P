namespace AUDIO_NEW
{
    partial class StartForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            timerFadeIn = new System.Windows.Forms.Timer(components);
            labelTitle = new Label();
            labelLoading = new Label();
            labelPercent = new Label();
            panelProgress = new Panel();
            panelLoading = new Panel();
            timerProgress = new System.Windows.Forms.Timer(components);
            timerFadeOut = new System.Windows.Forms.Timer(components);
            panelProgress.SuspendLayout();
            SuspendLayout();
            // 
            // timerFadeIn
            // 
            timerFadeIn.Interval = 10;
            timerFadeIn.Tick += timerFadeIn_Tick;
            // 
            // labelTitle
            // 
            labelTitle.AutoSize = true;
            labelTitle.BackColor = Color.Transparent;
            labelTitle.Font = new Font("Franklin Gothic Medium", 24F, FontStyle.Bold, GraphicsUnit.Point, 0);
            labelTitle.Location = new Point(196, 212);
            labelTitle.Name = "labelTitle";
            labelTitle.Size = new Size(341, 37);
            labelTitle.TabIndex = 2;
            labelTitle.Text = "CHAT VOICE CONNECT";
            labelTitle.Click += labelTitle_Click;
            // 
            // labelLoading
            // 
            labelLoading.AutoSize = true;
            labelLoading.BackColor = Color.Transparent;
            labelLoading.Font = new Font("Segoe UI", 14.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
            labelLoading.ForeColor = Color.Black;
            labelLoading.Location = new Point(308, 355);
            labelLoading.Name = "labelLoading";
            labelLoading.Size = new Size(112, 25);
            labelLoading.TabIndex = 3;
            labelLoading.Text = "Initializing...";
            // 
            // labelPercent
            // 
            labelPercent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            labelPercent.AutoSize = true;
            labelPercent.BackColor = Color.Transparent;
            labelPercent.Cursor = Cursors.AppStarting;
            labelPercent.Font = new Font("Segoe UI", 11.25F, FontStyle.Regular, GraphicsUnit.Point, 163);
            labelPercent.Location = new Point(349, 11);
            labelPercent.Name = "labelPercent";
            labelPercent.Size = new Size(29, 20);
            labelPercent.TabIndex = 10;
            labelPercent.Text = "0%";
            // 
            // panelProgress
            // 
            panelProgress.BackColor = Color.LightPink;
            panelProgress.Controls.Add(panelLoading);
            panelProgress.Controls.Add(labelPercent);
            panelProgress.Dock = DockStyle.Bottom;
            panelProgress.Location = new Point(0, 410);
            panelProgress.Name = "panelProgress";
            panelProgress.Size = new Size(737, 40);
            panelProgress.TabIndex = 0;
            // 
            // panelLoading
            // 
            panelLoading.BackColor = SystemColors.ControlLight;
            panelLoading.Location = new Point(0, 0);
            panelLoading.Name = "panelLoading";
            panelLoading.Size = new Size(0, 40);
            panelLoading.TabIndex = 0;
            // 
            // timerProgress
            // 
            timerProgress.Interval = 40;
            timerProgress.Tick += timerProgress_Tick;
            // 
            // timerFadeOut
            // 
            timerFadeOut.Interval = 10;
            timerFadeOut.Tick += timerFadeOut_Tick;
            // 
            // StartForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.ActiveCaption;
            BackgroundImage = Properties.Resources.img_17;
            ClientSize = new Size(737, 450);
            ControlBox = false;
            Controls.Add(panelProgress);
            Controls.Add(labelLoading);
            Controls.Add(labelTitle);
            FormBorderStyle = FormBorderStyle.None;
            Name = "StartForm";
            Opacity = 0D;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Form1";
            panelProgress.ResumeLayout(false);
            panelProgress.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion
        private System.Windows.Forms.Timer timerFadeIn;
        private Label labelTitle;
        private Label labelLoading;
        private Label labelPercent;
        private Panel panelProgress;
        private System.Windows.Forms.Timer timerProgress;
        private System.Windows.Forms.Timer timerFadeOut;
        private Panel panelLoading;
    }
}
