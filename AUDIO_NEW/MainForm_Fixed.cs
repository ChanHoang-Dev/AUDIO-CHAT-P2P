using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace AUDIO_NEW
{
    public partial class MainForm : Form
    {
        // Managers
        private NetworkManager networkManager;
        private PeerManager peerManager;
        private UserDataManager userDataManager;
        private CallManager callManager;
        private ChatManagerTCP chatManager;
        private ChatTCPServer chatServer;
        private SaveVoice saveVoice;

        // Current state
        private string currentCallTarget = "";
        private int displayMessageCount = 0;

        public MainForm(string username, string displayName = "", string gender = "Khác")
        {
            InitializeComponent();

            // Kh?i t?o User Data Manager
            userDataManager = new UserDataManager(username, displayName, gender);

            // Kh?i t?o Network Manager
            int localAudioPort = GetAvailableUdpPort();
            networkManager = new NetworkManager(
                username,
                userDataManager.DisplayName,
                userDataManager.UserGender,
                localAudioPort
            );

            // Kh?i t?o Peer Manager
            peerManager = new PeerManager();
            peerManager.PeerListUpdated += OnPeerListUpdated;

            // Kh?i t?o Call Manager
            callManager = new CallManager(this, networkManager, peerManager);

            // Subscribe to network events
            networkManager.PeerDiscovered += OnPeerDiscovered;

            // UI Setup
            SetupUI();

            // Start network services
            networkManager.StartBroadcast();
            networkManager.StartListener();

            // Start chat server
            StartChatServer();

            Debug.WriteLine($"[APP START] username={username}, gender={userDataManager.UserGender}, port={localAudioPort}");
        }

        private void SetupUI()
        {
            picCall.Image = new Bitmap(Properties.Resources.Call, new Size(50, 50));
            picExit.Image = new Bitmap(Properties.Resources.Exit, new Size(30, 30));
            picSetting.Image = new Bitmap(Properties.Resources.Setting, new Size(30, 30));

            picAvatar1.Image = UIHelper.GetAvatarByGender(userDataManager.UserGender);
            picGroup.Image = new Bitmap(Properties.Resources.Group, new Size(50, 50));

            picSaveVoice.Image = new Bitmap(Properties.Resources.SaveVoice, new Size(30, 30));

            pnInteract.Visible = false;
            panelGroup.Visible = false;

            // ListView setup
            lsvFriend.View = View.Details;
            lsvFriend.Columns.Add("Tên b?n bè", 150);
            lsvFriend.Columns.Add("Tr?ng thái", 80);
            lsvFriend.Columns.Add("Ð?a ch? IP", 120);
            lsvFriend.FullRowSelect = true;
            lsvFriend.MultiSelect = false;
            lsvFriend.HideSelection = false;
        }

        private int GetAvailableUdpPort()
        {
            var udp = new UdpClient(0);
            int port = ((IPEndPoint)udp.Client.LocalEndPoint).Port;
            udp.Close();
            return port;
        }

        #region Chat Management

        private void StartChatServer()
        {
            chatServer = new ChatTCPServer();
            chatServer.MessageReceived += (s, msg) =>
            {
                this.Invoke(new Action(() =>
                {
                    //  LÝU MESSAGE TRÝ?C KHI HI?N TH?
                    if (chatManager != null)
                    {
                        try
                        {
                            chatManager.SaveReceivedMessage(msg);
                            Debug.WriteLine($"[CHAT] Saved incoming message from {msg.SenderId}");
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"[CHAT] Error saving: {ex.Message}");
                        }
                    }

                    msg.IsOwn = false;
                    DisplayChatMessage(msg);
                }));
            };
            chatServer.Start();
        }

        public void StartChat(string userId, IPAddress remoteIP)
        {
            try
            {
                chatManager = new ChatManagerTCP(userDataManager.GetUserDataPath(), userId);

                bool connected = chatManager.ConnectToRemote(remoteIP);
                if (!connected)
                {
                    MessageBox.Show("Không th? k?t n?i ð? chat.");
                    return;
                }

                flpMessage.Controls.Clear();

                List<ChatMessage> history = chatManager.GetChatHistory();
                foreach (var msg in history)
                {
                    msg.IsOwn = (msg.SenderId == networkManager.UserName);
                    DisplayChatMessage(msg);
                }

                chatManager.MessageReceived -= ChatManager_MessageReceived;
                chatManager.MessageReceived += ChatManager_MessageReceived;
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi b?t ð?u chat: " + ex.Message);
            }
        }

        private void ChatManager_MessageReceived(object sender, ChatMessage msg)
        {
            this.Invoke(new Action(() =>
            {
                if (msg.SenderId == networkManager.UserName)
                {
                    Debug.WriteLine($"[CHAT] Skipped own message echo");
                    return;
                }
                msg.IsOwn = false;
                DisplayChatMessage(msg);
            }));
        }

        public void SendChatMessage(string recipientId, string content, IPAddress recipientIP)
        {
            if (chatManager == null)
            {
                MessageBox.Show("Vui l?ng ch?n ngý?i ð? chat trý?c.");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Vui l?ng nh?p tin nh?n!");
                return;
            }

            try
            {
                ChatMessage msg = new ChatMessage(
                    networkManager.UserName,
                    userDataManager.DisplayName,
                    recipientId,
                    content
                );
                msg.IsOwn = true;

                chatManager.SendMessage(msg);
                DisplayChatMessage(msg);
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi g?i tin nh?n: " + ex.Message);
            }
        }

        private void DisplayChatMessage(ChatMessage message)
        {
            try
            {
                displayMessageCount++;
                ChatMessageBubble bubble = new ChatMessageBubble(message);
                flpMessage.Controls.Add(bubble);
                flpMessage.VerticalScroll.Value = flpMessage.VerticalScroll.Maximum;
                flpMessage.PerformLayout();
                flpMessage.Update();
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i hi?n th? tin nh?n: " + ex.Message);
            }
        }

        public void OnSendChatButtonClick(string messageContent)
        {
            if (string.IsNullOrEmpty(currentCallTarget))
            {
                MessageBox.Show("Vui l?ng ch?n ngý?i ð? chat!");
                return;
            }

            var peerEndpoint = peerManager.GetPeerEndpoint(currentCallTarget);
            if (peerEndpoint == null)
            {
                MessageBox.Show("Ngý?i này không online.");
                return;
            }

            SendChatMessage(currentCallTarget, messageContent, peerEndpoint.Address);
        }

        #endregion

        #region Network Events

        private void OnPeerDiscovered(object sender, PeerInfo peerInfo)
        {
            peerManager.AddOrUpdatePeer(peerInfo);
        }

        private void OnPeerListUpdated(object sender, List<PeerInfo> peers)
        {
            this.Invoke(new Action(() =>
            {
                UIHelper.UpdatePeerListView(lsvFriend, peers);
            }));
        }

        #endregion

        #region UI Events

        private void MainForm_Load(object sender, EventArgs e)
        {
            Debug.WriteLine($"[APP] MainForm loaded");
        }

        private void lsvFriend_Click(object sender, EventArgs e)
        {
            if (lsvFriend.SelectedItems.Count == 0) return;

            ListViewItem selectedItem = lsvFriend.SelectedItems[0];
            string selectedUsername = selectedItem.Tag?.ToString();
            string selectedDisplayName = selectedItem.SubItems[0].Text;
            string selectedIP = selectedItem.SubItems[2].Text;

            if (string.IsNullOrEmpty(selectedUsername))
            {
                MessageBox.Show("L?i: Không l?y ðý?c thông tin ngý?i dùng.");
                return;
            }

            currentCallTarget = selectedUsername;
            pnInteract.Visible = true;
            lbReceiverCall.Text = selectedDisplayName;
            picAvatar2.Image = picAvatar1.Image;

            if (IPAddress.TryParse(selectedIP, out IPAddress remoteIP))
            {
                StartChat(selectedUsername, remoteIP);
            }
            else
            {
                MessageBox.Show("Ð?a ch? IP không h?p l?: " + selectedIP);
            }
        }

        private void lsvFriend_SelectedIndexChanged(object sender, EventArgs e)
        {
            lsvFriend_Click(sender, e);
        }

        private void picCall_Click(object sender, EventArgs e)
        {
            // ? CHECK: Ðang trong cu?c g?i?
            if (callManager.Audio != null)
            {
                MessageBox.Show("B?n ðang trong cu?c g?i!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (lsvFriend.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui l?ng ch?n m?t ngý?i b?n ð? g?i.");
                return;
            }

            ListViewItem item = lsvFriend.SelectedItems[0];
            string targetUsername = item.Tag?.ToString();
            string targetDisplayName = item.SubItems[0].Text;

            if (string.IsNullOrEmpty(targetUsername))
            {
                MessageBox.Show("L?i: Không l?y ðý?c thông tin ngý?i dùng.");
                return;
            }

            if (targetUsername == networkManager.UserName)
            {
                MessageBox.Show("B?n không th? g?i cho chính m?nh!");
                return;
            }

            var peerEndpoint = peerManager.GetPeerEndpoint(targetUsername);
            if (peerEndpoint == null)
            {
                MessageBox.Show("Không t?m th?y peer trong LAN.");
                return;
            }

            var confirm = MessageBox.Show(
                $"B?n có ch?c ch?n mu?n g?i cho {targetDisplayName}?",
                "Xác nh?n cu?c g?i",
                MessageBoxButtons.YesNo
            );
            if (confirm == DialogResult.No) return;

            // L?y gender c?a ngý?i nh?n
            var peer = peerManager.GetPeer(targetUsername);
            string targetGender = peer?.Gender ?? "Khác";

            callManager.StartOutgoingCall(targetUsername, targetDisplayName, targetGender, peerEndpoint);
        }

        private void btnSendMessage_Click(object sender, EventArgs e)
        {
            SendMessage();
        }
        private void picSaveVoice_Click(object sender, EventArgs e)
        {
            try
            {
                // M? SaveVoice form ð? ghi âm
                saveVoice = new SaveVoice(this, userDataManager.GetUserDataPath());
                saveVoice.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi m? form ghi âm: {ex.Message}", "L?i", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine($"[VOICE ERROR] picSaveVoice_Click: {ex.Message}");
            }
        }

        private void SendMessage()
        {
            if (txtMessage == null) return;
            string messageContent = txtMessage.Text.Trim();
            if (string.IsNullOrWhiteSpace(messageContent)) return;

            try
            {
                OnSendChatButtonClick(messageContent);
                txtMessage.Clear();
                txtMessage.Focus();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i: {ex.Message}");
            }
        }
        private void picGroup_Click(object sender, EventArgs e)
        {
            panelGroup.Visible = true;
        }

        private void picSetting_Click(object sender, EventArgs e)
        {
            // TODO: Implement settings
        }

        private void picExit_Click(object sender, EventArgs e)
        {
            DialogResult result = MessageBox.Show(
                "B?n có ch?c ch?n mu?n thoát?",
                "Xác nh?n",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            );

            if (result == DialogResult.Yes)
            {
                Cleanup();
                Application.Exit();
            }
        }

        #endregion

        #region Public Methods

        public void SendEndCall(string targetUsername)
        {
            callManager.EndCall(targetUsername);
        }

        public void CloseCallInterface(callinterface ci)
        {
            this.Invoke(new Action(() =>
            {
                if (ci != null && ci.Visible)
                {
                    ci.Close();
                }
                this.Show();
            }));
        }

        public void SaveCallHistory(string targetUser, string targetDisplayName, string callType, TimeSpan duration)
        {
            try
            {
                userDataManager.SaveCallHistory(targetUser, targetDisplayName, callType, duration);
            }
            catch (Exception ex)
            {
                MessageBox.Show("L?i khi lýu l?ch s?: " + ex.Message);
            }
        }

        /// <summary>
        /// G?i voice message t?i peer (g?i metadata + file .wav + .txt)
        /// </summary>
        public void SendVoiceMessage(VoiceMessage voiceMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(currentCallTarget))
                {
                    MessageBox.Show("Vui l?ng ch?n ngý?i ð? g?i tin nh?n.");
                    return;
                }

                // G?i qua chatManager (metadata + files)
                if (chatManager != null)
                {
                    chatManager.SendVoiceMessage(voiceMessage);
                    Debug.WriteLine($"[VOICE] SendVoiceMessage: {voiceMessage.Id} to {currentCallTarget}");
                }
                
                // Hi?n th? trong chat c?a m?nh
                DisplayVoiceMessage(voiceMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i khi g?i voice message: {ex.Message}", "L?i");
                Debug.WriteLine($"[VOICE ERROR] SendVoiceMessage: {ex.Message}");
            }
        }

        /// <summary>
        /// Hi?n th? voice message trong FlowLayoutPanel
        /// </summary>
        public void DisplayVoiceMessage(VoiceMessage voiceMessage)
        {
            try
            {
                this.Invoke(new Action(() =>
                {
                    VoiceMessageBubble bubble = new VoiceMessageBubble(voiceMessage);
                    flpMessage.Controls.Add(bubble);
                    flpMessage.VerticalScroll.Value = flpMessage.VerticalScroll.Maximum;
                    flpMessage.PerformLayout();
                    flpMessage.Update();

                    Debug.WriteLine($"[VOICE] Displayed voice message: {voiceMessage.Id}");
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"L?i hi?n th? voice message: {ex.Message}", "L?i");
                Debug.WriteLine($"[VOICE ERROR] DisplayVoiceMessage: {ex.Message}");
            }
        }

        /// <summary>
        /// Getter: L?y username hi?n t?i
        /// </summary>
        public string GetUserName()
        {
            return networkManager.UserName;
        }

        /// <summary>
        /// Getter: L?y display name c?a user
        /// </summary>
        public string GetDisplayName()
        {
            return userDataManager.DisplayName;
        }

        /// <summary>
        /// Getter: L?y username c?a ngý?i chat hi?n t?i
        /// </summary>
        public string GetCurrentCallTarget()
        {
            return currentCallTarget;
        }

        #endregion

        #region Cleanup

        private void Cleanup()
        {
            try
            {
                chatServer?.Stop();
                chatManager?.Disconnect();
                networkManager?.Stop();
                peerManager?.Stop();
                callManager?.Cleanup();
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[CLEANUP ERROR] {ex.Message}");
            }
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);
            Cleanup();
        }

        #endregion
    }
}
