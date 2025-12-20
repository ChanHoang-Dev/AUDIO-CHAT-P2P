namespace AUDIO_NEW
{
    partial class MainForm
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
            pnInfor = new Panel();
            picGroup = new PictureBox();
            groupBox1 = new GroupBox();
            lsvFriend = new ListView();
            picExit = new PictureBox();
            picSetting = new PictureBox();
            picAvatar1 = new PictureBox();
            pnInteract = new Panel();
            panel2 = new Panel();
            picSaveVoice = new PictureBox();
            btnSendMessage = new Button();
            txtMessage = new TextBox();
            panel1 = new Panel();
            panelGroup = new Panel();
            picAvatar2 = new PictureBox();
            picCall = new PictureBox();
            lbReceiverCall = new Label();
            flpMessage = new FlowLayoutPanel();
            pnInfor.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picGroup).BeginInit();
            groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picExit).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picSetting).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picAvatar1).BeginInit();
            pnInteract.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picSaveVoice).BeginInit();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar2).BeginInit();
            ((System.ComponentModel.ISupportInitialize)picCall).BeginInit();
            SuspendLayout();
            // 
            // pnInfor
            // 
            pnInfor.BackColor = Color.Pink;
            pnInfor.Controls.Add(picGroup);
            pnInfor.Controls.Add(groupBox1);
            pnInfor.Controls.Add(picExit);
            pnInfor.Controls.Add(picSetting);
            pnInfor.Controls.Add(picAvatar1);
            pnInfor.Dock = DockStyle.Left;
            pnInfor.Location = new Point(0, 0);
            pnInfor.Name = "pnInfor";
            pnInfor.Size = new Size(276, 490);
            pnInfor.TabIndex = 1;
            // 
            // picGroup
            // 
            picGroup.Location = new Point(12, 82);
            picGroup.Name = "picGroup";
            picGroup.Size = new Size(50, 50);
            picGroup.TabIndex = 7;
            picGroup.TabStop = false;
            picGroup.Click += picGroup_Click;
            // 
            // groupBox1
            // 
            groupBox1.Controls.Add(lsvFriend);
            groupBox1.Font = new Font("Segoe UI", 12F, FontStyle.Bold, GraphicsUnit.Point, 163);
            groupBox1.Location = new Point(68, 3);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new Size(205, 484);
            groupBox1.TabIndex = 6;
            groupBox1.TabStop = false;
            groupBox1.Text = "Friend";
            // 
            // lsvFriend
            // 
            lsvFriend.Location = new Point(6, 22);
            lsvFriend.Name = "lsvFriend";
            lsvFriend.Size = new Size(196, 456);
            lsvFriend.TabIndex = 0;
            lsvFriend.UseCompatibleStateImageBehavior = false;
            lsvFriend.SelectedIndexChanged += lsvFriend_SelectedIndexChanged;
            lsvFriend.Click += lsvFriend_Click;
            // 
            // picExit
            // 
            picExit.Location = new Point(12, 439);
            picExit.Name = "picExit";
            picExit.Size = new Size(30, 30);
            picExit.TabIndex = 3;
            picExit.TabStop = false;
            picExit.Click += picExit_Click;
            // 
            // picSetting
            // 
            picSetting.Location = new Point(12, 400);
            picSetting.Name = "picSetting";
            picSetting.Size = new Size(30, 30);
            picSetting.TabIndex = 2;
            picSetting.TabStop = false;
            picSetting.Click += picSetting_Click;
            // 
            // picAvatar1
            // 
            picAvatar1.Location = new Point(12, 8);
            picAvatar1.Name = "picAvatar1";
            picAvatar1.Size = new Size(50, 50);
            picAvatar1.TabIndex = 1;
            picAvatar1.TabStop = false;
            // 
            // pnInteract
            // 
            pnInteract.Controls.Add(panel2);
            pnInteract.Controls.Add(panel1);
            pnInteract.Controls.Add(flpMessage);
            pnInteract.Dock = DockStyle.Right;
            pnInteract.Location = new Point(275, 0);
            pnInteract.Name = "pnInteract";
            pnInteract.Size = new Size(572, 490);
            pnInteract.TabIndex = 2;
            // 
            // panel2
            // 
            panel2.BackColor = Color.Pink;
            panel2.Controls.Add(picSaveVoice);
            panel2.Controls.Add(btnSendMessage);
            panel2.Controls.Add(txtMessage);
            panel2.Dock = DockStyle.Bottom;
            panel2.Location = new Point(0, 439);
            panel2.Name = "panel2";
            panel2.Size = new Size(572, 51);
            panel2.TabIndex = 6;
            // 
            // picSaveVoice
            // 
            picSaveVoice.Location = new Point(35, 12);
            picSaveVoice.Name = "picSaveVoice";
            picSaveVoice.Size = new Size(30, 30);
            picSaveVoice.TabIndex = 2;
            picSaveVoice.TabStop = false;
            picSaveVoice.Click += picSaveVoice_Click;
            // 
            // btnSendMessage
            // 
            btnSendMessage.BackColor = Color.PaleVioletRed;
            btnSendMessage.Location = new Point(473, 14);
            btnSendMessage.Name = "btnSendMessage";
            btnSendMessage.Size = new Size(75, 23);
            btnSendMessage.TabIndex = 1;
            btnSendMessage.Text = "Send";
            btnSendMessage.UseVisualStyleBackColor = false;
            btnSendMessage.Click += btnSendMessage_Click;
            // 
            // txtMessage
            // 
            txtMessage.Location = new Point(98, 15);
            txtMessage.Name = "txtMessage";
            txtMessage.Size = new Size(359, 23);
            txtMessage.TabIndex = 0;
            // 
            // panel1
            // 
            panel1.Controls.Add(panelGroup);
            panel1.Controls.Add(picAvatar2);
            panel1.Controls.Add(picCall);
            panel1.Controls.Add(lbReceiverCall);
            panel1.Dock = DockStyle.Top;
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(572, 100);
            panel1.TabIndex = 5;
            // 
            // panelGroup
            // 
            panelGroup.BackColor = Color.Pink;
            panelGroup.Location = new Point(1, 0);
            panelGroup.Name = "panelGroup";
            panelGroup.Size = new Size(571, 385);
            panelGroup.TabIndex = 10;
            // 
            // picAvatar2
            // 
            picAvatar2.Location = new Point(25, 33);
            picAvatar2.Name = "picAvatar2";
            picAvatar2.Size = new Size(40, 40);
            picAvatar2.TabIndex = 2;
            picAvatar2.TabStop = false;
            // 
            // picCall
            // 
            picCall.Location = new Point(427, 23);
            picCall.Name = "picCall";
            picCall.Size = new Size(50, 50);
            picCall.TabIndex = 3;
            picCall.TabStop = false;
            picCall.Click += picCall_Click;
            // 
            // lbReceiverCall
            // 
            lbReceiverCall.AutoSize = true;
            lbReceiverCall.Location = new Point(98, 43);
            lbReceiverCall.Name = "lbReceiverCall";
            lbReceiverCall.Size = new Size(38, 15);
            lbReceiverCall.TabIndex = 1;
            lbReceiverCall.Text = "label3";
            // 
            // flpMessage
            // 
            flpMessage.AutoScroll = true;
            flpMessage.AutoScrollMinSize = new Size(0, 1);
            flpMessage.BackColor = SystemColors.ActiveCaption;
            flpMessage.BackgroundImage = Properties.Resources.img_8;
            flpMessage.FlowDirection = FlowDirection.TopDown;
            flpMessage.Location = new Point(1, 100);
            flpMessage.Name = "flpMessage";
            flpMessage.Size = new Size(569, 339);
            flpMessage.TabIndex = 0;
            flpMessage.WrapContents = false;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(847, 490);
            Controls.Add(pnInteract);
            Controls.Add(pnInfor);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "MainForm";
            Load += MainForm_Load;
            pnInfor.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picGroup).EndInit();
            groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)picExit).EndInit();
            ((System.ComponentModel.ISupportInitialize)picSetting).EndInit();
            ((System.ComponentModel.ISupportInitialize)picAvatar1).EndInit();
            pnInteract.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picSaveVoice).EndInit();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)picAvatar2).EndInit();
            ((System.ComponentModel.ISupportInitialize)picCall).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Button btnExit;
        private Panel pnInfor;
        private PictureBox picAvatar1;
        private GroupBox groupBox1;
        private ListView lsvFriend;
        private PictureBox picExit;
        private PictureBox picSetting;
        private Panel pnInteract;
        private PictureBox picAvatar2;
        private Label lbReceiverCall;
        private Button button1;
        private PictureBox picCall;
        private Panel panel2;
        private Panel panel1;
        private FlowLayoutPanel flpMessage;
        private TextBox txtMessage;
        private Button btnSendMessage;
        private PictureBox picGroup;
        private Panel panelGroup;
        private PictureBox picSaveVoice;
    }
}