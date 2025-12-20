using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace AUDIO_NEW
{
    public partial class ChatMessageBubble : UserControl
    {
        private bool isOwn;
        private ChatMessage message;
        private Label lblSenderName;
        private Label lblContent;
        private Label lblTime;

        public ChatMessageBubble(ChatMessage message)
        {
            this.isOwn = message.IsOwn;
            this.message = message;
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            BuildUI(message);
        }
        private void BuildUI(ChatMessage message)
        {
            this.BackColor = Color.Transparent;
            this.Width = 400;

            lblSenderName = new Label();
            lblSenderName.Text = message.SenderName;
            lblSenderName.Font = new Font("Arial", 10, FontStyle.Bold);
            lblSenderName.AutoSize = true;
            lblSenderName.Location = new Point(isOwn ? 300 : 10, 5);
            this.Controls.Add(lblSenderName);

            lblContent = new Label();
            lblContent.Text = message.Content;
            lblContent.Font = new Font("Arial", 11);
            lblContent.MaximumSize = new Size(350, 200);
            lblContent.AutoSize = true;
            lblContent.Padding = new Padding(10, 8, 10, 8);

            Panel bubblePanel = new Panel();
            bubblePanel.BackColor = isOwn ? Color.LightBlue : Color.LightGray;
            bubblePanel.Controls.Add(lblContent);
            bubblePanel.AutoSize = true;
            bubblePanel.Location = new Point(isOwn ? 400 - 20 - lblContent.Width : 10, 30);
            this.Controls.Add(bubblePanel);

            lblTime = new Label();
            lblTime.Text = message.Timestamp.ToString("HH:mm:ss");
            lblTime.Font = new Font("Arial", 9);
            lblTime.AutoSize = true;
            lblTime.ForeColor = Color.Gray;
            lblTime.Location = new Point(bubblePanel.Left, bubblePanel.Bottom + 5);
            this.Controls.Add(lblTime);

            this.Height = lblTime.Bottom + 10;
        }

    }
}
