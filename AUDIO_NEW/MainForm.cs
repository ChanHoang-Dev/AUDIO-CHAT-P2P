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

            // Khởi tạo User Data Manager
            userDataManager = new UserDataManager(username, displayName, gender);

            // Khởi tạo Network Manager
            int localAudioPort = GetAvailableUdpPort();
            networkManager = new NetworkManager(
                username,
                userDataManager.DisplayName,
                userDataManager.UserGender,
                localAudioPort
            );

            // Khởi tạo Peer Manager
            peerManager = new PeerManager();
            peerManager.PeerListUpdated += OnPeerListUpdated;

            // Khởi tạo Call Manager
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
            lsvFriend.Columns.Add("Tên bạn bè", 150);
            lsvFriend.Columns.Add("Trạng thái", 80);
            lsvFriend.Columns.Add("Địa chỉ IP", 120);
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
            chatServer = new ChatTCPServer(userDataManager.GetUserDataPath());

            chatServer.MessageReceived += (s, msg) =>
            {
                this.Invoke(new Action(() =>
                {
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

            chatServer.VoiceMessageReceived += (s, voiceMsg) =>
            {
                Debug.WriteLine($"[VOICE MAINFORM] VoiceMessageReceived event fired for ID: {voiceMsg.Id}");
                Debug.WriteLine($"[VOICE MAINFORM] SenderId: {voiceMsg.SenderId}, RecipientId: {voiceMsg.RecipientId}");
                Debug.WriteLine($"[VOICE MAINFORM] AudioFileName: {voiceMsg.AudioFileName}");
                Debug.WriteLine($"[VOICE MAINFORM] TranscriptionFileName: {voiceMsg.TranscriptionFileName}");
                Debug.WriteLine($"[VOICE MAINFORM] UserDataPath: {voiceMsg.UserDataPath}");

                try
                {
                    // Kiểm tra thread
                    if (this.InvokeRequired)
                    {
                        Debug.WriteLine($"[VOICE MAINFORM] InvokeRequired = true, invoking to UI thread");
                        this.Invoke(new Action(() =>
                        {
                            DisplayReceivedVoiceMessage(voiceMsg);
                        }));
                    }
                    else
                    {
                        Debug.WriteLine($"[VOICE MAINFORM] Already on UI thread");
                        DisplayReceivedVoiceMessage(voiceMsg);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[VOICE ERROR MAINFORM] Exception: {ex.Message}");
                    Debug.WriteLine($"[VOICE ERROR MAINFORM] StackTrace: {ex.StackTrace}");
                    MessageBox.Show($"Lỗi hiển thị voice message: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };

            chatServer.Start();
            Debug.WriteLine($"[CHAT SERVER] Started on port {ChatTCPServer.SERVER_PORT}");
        }

        private void DisplayReceivedVoiceMessage(VoiceMessage voiceMsg)
        {
            try
            {
                Debug.WriteLine($"[VOICE MAINFORM] DisplayReceivedVoiceMessage called");

                // Kiểm tra file tồn tại
                string audioPath = System.IO.Path.Combine(voiceMsg.UserDataPath, "voice_messages", voiceMsg.AudioFileName);
                string transcriptionPath = System.IO.Path.Combine(voiceMsg.UserDataPath, "voice_messages", voiceMsg.TranscriptionFileName);

                Debug.WriteLine($"[VOICE MAINFORM] Checking audioPath: {audioPath}");
                Debug.WriteLine($"[VOICE MAINFORM] Audio exists: {System.IO.File.Exists(audioPath)}");

                Debug.WriteLine($"[VOICE MAINFORM] Checking transcriptionPath: {transcriptionPath}");
                Debug.WriteLine($"[VOICE MAINFORM] Transcription exists: {System.IO.File.Exists(transcriptionPath)}");

                if (!System.IO.File.Exists(audioPath))
                {
                    Debug.WriteLine($"[VOICE ERROR] Audio file not found: {audioPath}");
                    MessageBox.Show($"Audio file not found: {audioPath}", "Lỗi");
                    return;
                }

                if (!System.IO.File.Exists(transcriptionPath))
                {
                    Debug.WriteLine($"[VOICE WARNING] Transcription file not found: {transcriptionPath}");
                }

                voiceMsg.IsOwn = false;

                Debug.WriteLine($"[VOICE MAINFORM] Creating VoiceMessageBubble");
                VoiceMessageBubble bubble = new VoiceMessageBubble(voiceMsg);

                Debug.WriteLine($"[VOICE MAINFORM] Adding bubble to flpMessage");
                flpMessage.Controls.Add(bubble);

                Debug.WriteLine($"[VOICE MAINFORM] Scrolling to bottom");
                flpMessage.VerticalScroll.Value = flpMessage.VerticalScroll.Maximum;
                flpMessage.PerformLayout();
                flpMessage.Update();

                Debug.WriteLine($"[VOICE MAINFORM] ? Successfully displayed received voice message: {voiceMsg.Id}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[VOICE ERROR] DisplayReceivedVoiceMessage exception: {ex.Message}");
                Debug.WriteLine($"[VOICE ERROR] StackTrace: {ex.StackTrace}");
                throw;
            }
        }

        public void StartChat(string userId, IPAddress remoteIP)
        {
            try
            {
                chatManager = new ChatManagerTCP(userDataManager.GetUserDataPath(), userId);

                bool connected = chatManager.ConnectToRemote(remoteIP);
                if (!connected)
                {
                    MessageBox.Show("Không thể kết nối để chat.");
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
                MessageBox.Show("Lỗi khi bắt đầu chat: " + ex.Message);
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
                MessageBox.Show("Vui lòng chọn người để chat trước.");
                return;
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                MessageBox.Show("Vui lòng nhập tin nhắn!");
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
                MessageBox.Show("Lỗi khi gửi tin nhắn: " + ex.Message);
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
                MessageBox.Show("Lỗi hiển thị tin nhắn: " + ex.Message);
            }
        }

        public void OnSendChatButtonClick(string messageContent)
        {
            if (string.IsNullOrEmpty(currentCallTarget))
            {
                MessageBox.Show("Vui lòng chọn người để chat!");
                return;
            }

            var peerEndpoint = peerManager.GetPeerEndpoint(currentCallTarget);
            if (peerEndpoint == null)
            {
                MessageBox.Show("Người này không online.");
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
                MessageBox.Show("Lỗi: Không lấy được thông tin người dùng.");
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
                MessageBox.Show("Địa chỉ IP không hợp lệ: " + selectedIP);
            }
        }

        private void lsvFriend_SelectedIndexChanged(object sender, EventArgs e)
        {
            lsvFriend_Click(sender, e);
        }

        private void picCall_Click(object sender, EventArgs e)
        {
            // ✅ CHECK: Đang trong cuộc gọi?
            if (callManager.Audio != null)
            {
                MessageBox.Show("Bạn đang trong cuộc gọi!",
                    "Thông báo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            if (lsvFriend.SelectedItems.Count == 0)
            {
                MessageBox.Show("Vui lòng chọn một người bạn để gọi.");
                return;
            }

            ListViewItem item = lsvFriend.SelectedItems[0];
            string targetUsername = item.Tag?.ToString();
            string targetDisplayName = item.SubItems[0].Text;

            if (string.IsNullOrEmpty(targetUsername))
            {
                MessageBox.Show("Lỗi: Không lấy được thông tin người dùng.");
                return;
            }

            if (targetUsername == networkManager.UserName)
            {
                MessageBox.Show("Bạn không thể gọi cho chính mình!");
                return;
            }

            var peerEndpoint = peerManager.GetPeerEndpoint(targetUsername);
            if (peerEndpoint == null)
            {
                MessageBox.Show("Không tìm thấy peer trong LAN.");
                return;
            }

            var confirm = MessageBox.Show(
                $"Bạn có chắc chắn muốn gọi cho {targetDisplayName}?",
                "Xác nhận cuộc gọi",
                MessageBoxButtons.YesNo
            );
            if (confirm == DialogResult.No) return;

            // Lấy gender của người nhận
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
                // Mở SaveVoice form để ghi âm
                saveVoice = new SaveVoice(this, userDataManager.GetUserDataPath());
                saveVoice.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi mở form ghi âm: {ex.Message}", "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
                MessageBox.Show($"Lỗi: {ex.Message}");
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
                "Bạn có chắc chắn muốn thoát?",
                "Xác nhận",
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
                MessageBox.Show("Lỗi khi lưu lịch sử: " + ex.Message);
            }
        }

        /// <summary>
        /// Gửi voice message tới peer (gửi metadata + file .wav + .txt)
        /// </summary>
        public void SendVoiceMessage(VoiceMessage voiceMessage)
        {
            try
            {
                if (string.IsNullOrEmpty(currentCallTarget))
                {
                    MessageBox.Show("Vui lòng chọn người để gửi tin nhắn.");
                    return;
                }

                Debug.WriteLine($"[VOICE MAINFORM] SendVoiceMessage called");
                Debug.WriteLine($"[VOICE MAINFORM] Current target: {currentCallTarget}");
                Debug.WriteLine($"[VOICE MAINFORM] VoiceMessage ID: {voiceMessage.Id}");

                // Gửi qua chatManager (metadata + files)
                if (chatManager != null)
                {
                    chatManager.SendVoiceMessage(voiceMessage);
                    Debug.WriteLine($"[VOICE MAINFORM] Sent via chatManager to {currentCallTarget}");
                }
                else
                {
                    Debug.WriteLine($"[VOICE ERROR] chatManager is null!");
                }

                // Display in own chat
                DisplayVoiceMessage(voiceMessage);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi gửi voice message: {ex.Message}", "Lỗi");
                Debug.WriteLine($"[VOICE ERROR] SendVoiceMessage: {ex.Message}");
            }
        }

        /// <summary>
        /// Hiển thị voice message trong FlowLayoutPanel (cho bên gửi)
        /// </summary>
        public void DisplayVoiceMessage(VoiceMessage voiceMessage)
        {
            try
            {
                Debug.WriteLine($"[VOICE MAINFORM] DisplayVoiceMessage (sender) called for ID: {voiceMessage.Id}");

                this.Invoke(new Action(() =>
                {
                    VoiceMessageBubble bubble = new VoiceMessageBubble(voiceMessage);
                    flpMessage.Controls.Add(bubble);
                    flpMessage.VerticalScroll.Value = flpMessage.VerticalScroll.Maximum;
                    flpMessage.PerformLayout();
                    flpMessage.Update();

                    Debug.WriteLine($"[VOICE MAINFORM] ? Displayed voice message (sender): {voiceMessage.Id}");
                }));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi hiển thị voice message: {ex.Message}", "Lỗi");
                Debug.WriteLine($"[VOICE ERROR] DisplayVoiceMessage: {ex.Message}");
            }
        }

        /// <summary>
        /// Getter: Lấy username hiện tại
        /// </summary>
        public string GetUserName()
        {
            return networkManager.UserName;
        }

        /// <summary>
        /// Getter: Lấy display name của user
        /// </summary>
        public string GetDisplayName()
        {
            return userDataManager.DisplayName;
        }

        /// <summary>
        /// Getter: Lấy username của người chat hiện tại
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