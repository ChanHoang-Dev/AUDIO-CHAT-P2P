using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AUDIO_NEW
{
    [Serializable]
    public class ChatMessage
    {
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string RecipientId { get; set; }
        public string Content { get; set; }
        public DateTime Timestamp { get; set; }

        [JsonIgnore]
        public bool IsOwn { get; set; }

        public ChatMessage() { }

        public ChatMessage(string senderId, string senderName, string recipientId, string content)
        {
            this.SenderId = senderId;
            this.SenderName = senderName;
            this.RecipientId = recipientId;
            this.Content = content;
            this.Timestamp = DateTime.Now;
            this.IsOwn = false;
        }

        public string ToMessage()
        {
            return $"CHAT_MESSAGE|{SenderId}|{SenderName}|{RecipientId}|{Content}|{Timestamp:yyyy-MM-dd HH:mm:ss}";
        }

        public static ChatMessage FromMessage(string message)
        {
            try
            {
                var parts = message.Split(new[] { '|' }, 6);
                if (parts.Length < 6) return null;

                return new ChatMessage
                {
                    SenderId = parts[1],
                    SenderName = parts[2],
                    RecipientId = parts[3],
                    Content = parts[4],
                    Timestamp = DateTime.Parse(parts[5])
                };
            }
            catch
            {
                return null;
            }
        }
    }

    public class ChatManagerTCP
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private string chatHistoryPath;
        private string myUserId;
        private string peerUserId;
        private const int CHAT_TCP_PORT = 5001;

        public event EventHandler<ChatMessage> MessageReceived;

        public ChatManagerTCP(string userDataPath, string peerUserId)
        {
            this.myUserId = Path.GetFileName(userDataPath);
            this.peerUserId = peerUserId;

            string chatPairId = GetChatPairId(myUserId, peerUserId);
            string chatFolder = Path.Combine(userDataPath, "chats");
            Directory.CreateDirectory(chatFolder);

            chatHistoryPath = Path.Combine(chatFolder, $"{chatPairId}.json");
        }

        private string GetChatPairId(string user1, string user2)
        {
            string[] users = { user1, user2 };
            Array.Sort(users);
            return $"{users[0]}_{users[1]}";
        }

        public bool ConnectToRemote(IPAddress remoteIP)
        {
            try
            {
                tcpClient = new TcpClient();
                tcpClient.Connect(remoteIP, CHAT_TCP_PORT);
                networkStream = tcpClient.GetStream();

                ListenForMessages();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("L?i k?t n?i TCP: " + ex.Message);
                return false;
            }
        }

        private void ListenForMessages()
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] buffer = new byte[4096];
                    StringBuilder messageBuffer = new StringBuilder();

                    while (tcpClient?.Connected == true)
                    {
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        messageBuffer.Append(data);

                        string messages = messageBuffer.ToString();
                        int newlineIndex;
                        while ((newlineIndex = messages.IndexOf('\n')) >= 0)
                        {
                            string singleMessage = messages.Substring(0, newlineIndex);
                            messages = messages.Substring(newlineIndex + 1);

                            if (!string.IsNullOrWhiteSpace(singleMessage) &&
                                singleMessage.StartsWith("CHAT_MESSAGE|"))
                            {
                                ChatMessage chatMsg = ChatMessage.FromMessage(singleMessage);
                                if (chatMsg != null)
                                {
                                    SaveMessage(chatMsg);
                                    MessageReceived?.Invoke(this, chatMsg);
                                }
                            }
                        }
                        messageBuffer.Clear();
                        messageBuffer.Append(messages);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("L?i nh?n tin nh?n: " + ex.Message);
                }
            });
        }

        public void SendMessage(ChatMessage message)
        {
            try
            {
                if (tcpClient?.Connected != true)
                    throw new Exception("K?t n?i TCP không ho?t đ?ng");

                string messageStr = message.ToMessage() + "\n";
                byte[] data = Encoding.UTF8.GetBytes(messageStr);

                networkStream.Write(data, 0, data.Length);
                networkStream.Flush();

                SaveMessage(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine("L?i g?i tin nh?n: " + ex.Message);
            }
        }

        private void SaveMessage(ChatMessage message)
        {
            try
            {
                List<ChatMessage> history = GetChatHistory();

                bool isDuplicate = history.Exists(m =>
                    m.SenderId == message.SenderId &&
                    m.Timestamp == message.Timestamp &&
                    m.Content == message.Content
                );

                if (!isDuplicate)
                {
                    history.Add(message);
                    string json = JsonConvert.SerializeObject(history, Formatting.Indented);
                    File.WriteAllText(chatHistoryPath, json);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CHAT] Error saving: {ex.Message}");
            }
        }

        public void SaveReceivedMessage(ChatMessage message)
        {
            SaveMessage(message);
        }

        public List<ChatMessage> GetChatHistory()
        {
            try
            {
                if (File.Exists(chatHistoryPath))
                {
                    string json = File.ReadAllText(chatHistoryPath);
                    var history = JsonConvert.DeserializeObject<List<ChatMessage>>(json);
                    return history ?? new List<ChatMessage>();
                }
            }
            catch { }

            return new List<ChatMessage>();
        }

        public void Disconnect()
        {
            try
            {
                networkStream?.Close();
                tcpClient?.Close();
            }
            catch { }
        }

        public void SendVoiceMessage(VoiceMessage voiceMessage)
        {
            try
            {
                if (tcpClient?.Connected != true)
                    throw new Exception("K?t n?i TCP không ho?t đ?ng");

                string baseFolder = GetBaseFolder();
                string audioPath = Path.Combine(baseFolder, "voice_messages", voiceMessage.AudioFileName);
                string transcriptionPath = Path.Combine(baseFolder, "voice_messages", voiceMessage.TranscriptionFileName);

                System.Diagnostics.Debug.WriteLine($"[CHAT] SendVoiceMessage - audioPath: {audioPath}, exists: {File.Exists(audioPath)}");
                System.Diagnostics.Debug.WriteLine($"[CHAT] SendVoiceMessage - transcPath: {transcriptionPath}, exists: {File.Exists(transcriptionPath)}");

                // Bư?c 1: G?i metadata
                string metadataStr = $"VOICE_MESSAGE|{voiceMessage.Id}|{voiceMessage.SenderId}|{voiceMessage.SenderName}|{voiceMessage.RecipientId}|{voiceMessage.Duration}|{voiceMessage.AudioFileName}|{voiceMessage.TranscriptionFileName}|{voiceMessage.Timestamp:yyyy-MM-dd HH:mm:ss}\n";
                byte[] metadataBytes = Encoding.UTF8.GetBytes(metadataStr);
                networkStream.Write(metadataBytes, 0, metadataBytes.Length);
                networkStream.Flush();
                System.Diagnostics.Debug.WriteLine($"[CHAT] Sent voice metadata: {metadataStr.Trim()}");

                // Chờ một chút để đảm bảo metadata được gửi
                System.Threading.Thread.Sleep(100);

                // Bư?c 2: G?i file .wav v?i kích thư?c
                if (File.Exists(audioPath))
                {
                    byte[] audioData = File.ReadAllBytes(audioPath);
                    // Format: VOICE_AUDIO|[id]|[size]\n[binary data]VOICE_AUDIO_END\n
                    string audioHeader = $"VOICE_AUDIO|{voiceMessage.Id}|{audioData.Length}\n";
                    byte[] audioHeaderBytes = Encoding.UTF8.GetBytes(audioHeader);

                    networkStream.Write(audioHeaderBytes, 0, audioHeaderBytes.Length);
                    networkStream.Flush();
                    System.Threading.Thread.Sleep(50);

                    networkStream.Write(audioData, 0, audioData.Length);
                    networkStream.Flush();
                    System.Threading.Thread.Sleep(50);

                    byte[] audioEndBytes = Encoding.UTF8.GetBytes("VOICE_AUDIO_END\n");
                    networkStream.Write(audioEndBytes, 0, audioEndBytes.Length);
                    networkStream.Flush();
                    System.Diagnostics.Debug.WriteLine($"[CHAT] Sent voice audio: {audioPath}, size: {audioData.Length}");

                    System.Threading.Thread.Sleep(100);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CHAT] ? Audio file not found: {audioPath}");
                }

                // Bư?c 3: G?i file .txt (transcription)
                if (File.Exists(transcriptionPath))
                {
                    string transcriptionContent = File.ReadAllText(transcriptionPath);
                    // Format: VOICE_TRANSCRIPTION|[id]|[size]\n[text content]VOICE_TRANSCRIPTION_END\n
                    string transcriptionHeader = $"VOICE_TRANSCRIPTION|{voiceMessage.Id}|{transcriptionContent.Length}\n";
                    byte[] transcriptionHeaderBytes = Encoding.UTF8.GetBytes(transcriptionHeader);

                    networkStream.Write(transcriptionHeaderBytes, 0, transcriptionHeaderBytes.Length);
                    networkStream.Flush();
                    System.Threading.Thread.Sleep(50);

                    byte[] transcriptionBytes = Encoding.UTF8.GetBytes(transcriptionContent);
                    networkStream.Write(transcriptionBytes, 0, transcriptionBytes.Length);
                    networkStream.Flush();
                    System.Threading.Thread.Sleep(50);

                    byte[] transcEndBytes = Encoding.UTF8.GetBytes("VOICE_TRANSCRIPTION_END\n");
                    networkStream.Write(transcEndBytes, 0, transcEndBytes.Length);
                    networkStream.Flush();
                    System.Diagnostics.Debug.WriteLine($"[CHAT] Sent voice transcription: {transcriptionPath}, size: {transcriptionContent.Length}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine($"[CHAT] ?? Transcription file not found: {transcriptionPath}");
                }

                System.Diagnostics.Debug.WriteLine($"[CHAT] ? Voice message sent completely: {voiceMessage.Id}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[CHAT] ? Error sending voice message: {ex.Message}");
                throw;
            }
        }

        private string GetBaseFolder()
        {
            return Path.GetDirectoryName(chatHistoryPath)?.Replace("\\chats", "") ?? "";
        }
    }

    public class ChatTCPServer
    {
        private TcpListener tcpListener;
        private List<ChatClientHandler> connectedClients = new List<ChatClientHandler>();
        private bool isRunning = false;
        private string userDataPath;

        public const int SERVER_PORT = 5001;
        public event EventHandler<ChatMessage> MessageReceived;
        public event EventHandler<VoiceMessage> VoiceMessageReceived;

        public ChatTCPServer(string userDataPath = null)
        {
            this.userDataPath = userDataPath;
        }

        public void Start()
        {
            try
            {
                tcpListener = new TcpListener(IPAddress.Any, SERVER_PORT);
                tcpListener.Start();
                isRunning = true;

                Task.Run(() =>
                {
                    while (isRunning)
                    {
                        try
                        {
                            TcpClient client = tcpListener.AcceptTcpClient();
                            ChatClientHandler handler = new ChatClientHandler(client, userDataPath);

                            handler.MessageReceived += (s, msg) =>
                            {
                                MessageReceived?.Invoke(this, msg);
                            };

                            handler.VoiceMessageReceived += (s, voiceMsg) =>
                            {
                                VoiceMessageReceived?.Invoke(this, voiceMsg);
                            };

                            connectedClients.Add(handler);
                            handler.Start();
                        }
                        catch { }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine("L?i kh?i t?o server: " + ex.Message);
            }
        }

        public void Stop()
        {
            isRunning = false;
            tcpListener?.Stop();

            foreach (var client in connectedClients.ToList())
            {
                client.Disconnect();
            }
            connectedClients.Clear();
        }
    }

    public class ChatClientHandler
    {
        private TcpClient tcpClient;
        private NetworkStream networkStream;
        private string userDataPath;

        public event EventHandler<ChatMessage> MessageReceived;
        public event EventHandler<VoiceMessage> VoiceMessageReceived;

        public ChatClientHandler(TcpClient client, string userDataPath = null)
        {
            this.tcpClient = client;
            this.networkStream = client.GetStream();
            this.userDataPath = userDataPath;
        }

        public void Start()
        {
            Task.Run(() =>
            {
                try
                {
                    byte[] buffer = new byte[65536];
                    byte[] dataBuffer = new byte[0];

                    while (tcpClient.Connected)
                    {
                        int bytesRead = networkStream.Read(buffer, 0, buffer.Length);
                        if (bytesRead == 0) break;

                        // N?i d? li?u vào buffer
                        byte[] newBuffer = new byte[dataBuffer.Length + bytesRead];
                        Buffer.BlockCopy(dataBuffer, 0, newBuffer, 0, dataBuffer.Length);
                        Buffer.BlockCopy(buffer, 0, newBuffer, dataBuffer.Length, bytesRead);
                        dataBuffer = newBuffer;

                        // X? l? voice message TRƯỚC (binary safe)
                        bool processed = ProcessVoiceMessage(ref dataBuffer);

                        // Chỉ x? l? chat nếu không có voice message
                        if (!processed)
                        {
                            ProcessChatMessages(ref dataBuffer);
                        }
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[CHAT] ChatClientHandler error: {ex.Message}");
                }
                finally
                {
                    Disconnect();
                }
            });
        }

        private void ProcessChatMessages(ref byte[] dataBuffer)
        {
            string text = Encoding.UTF8.GetString(dataBuffer);
            int idx = 0;
            int processedIdx = 0;

            while (true)
            {
                int newlineIdx = text.IndexOf('\n', idx);
                if (newlineIdx < 0) break;

                string line = text.Substring(idx, newlineIdx - idx);

                // ?? SKIP VOICE_MESSAGE lines
                if (line.StartsWith("VOICE_MESSAGE|") ||
                    line.StartsWith("VOICE_AUDIO|") ||
                    line.StartsWith("VOICE_TRANSCRIPTION|"))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOICE] Skipping voice-related line in ProcessChatMessages");
                    break;
                }

                if (line.StartsWith("CHAT_MESSAGE|"))
                {
                    ChatMessage msg = ChatMessage.FromMessage(line);
                    if (msg != null)
                    {
                        MessageReceived?.Invoke(this, msg);
                    }
                }

                processedIdx = newlineIdx + 1;
                idx = newlineIdx + 1;
            }

            // C?p nh?t buffer - gi? ph?n chưa x? l?
            if (processedIdx > 0)
            {
                byte[] remaining = new byte[dataBuffer.Length - processedIdx];
                Buffer.BlockCopy(dataBuffer, processedIdx, remaining, 0, remaining.Length);
                dataBuffer = remaining;
            }
        }

        private bool ProcessVoiceMessage(ref byte[] dataBuffer)
        {
            if (dataBuffer.Length < 20) return false;

            string headerText = Encoding.UTF8.GetString(dataBuffer, 0, Math.Min(50, dataBuffer.Length));

            if (!headerText.StartsWith("VOICE_MESSAGE|"))
            {
                return false;
            }

            System.Diagnostics.Debug.WriteLine($"[VOICE] Processing voice message, buffer size: {dataBuffer.Length}");

            // T?m metadata line (k?t thúc b?ng \n)
            int metaEndIdx = -1;
            for (int i = 0; i < dataBuffer.Length; i++)
            {
                if (dataBuffer[i] == '\n')
                {
                    metaEndIdx = i;
                    break;
                }
            }

            if (metaEndIdx < 0)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE] Waiting for complete metadata");
                return true; // Đang xử lý voice message
            }

            string metadataLine = Encoding.UTF8.GetString(dataBuffer, 0, metaEndIdx);
            System.Diagnostics.Debug.WriteLine($"[VOICE] Metadata: {metadataLine}");

            // Parse metadata
            string[] parts = metadataLine.Split('|');
            if (parts.Length < 9)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE] Invalid metadata parts: {parts.Length}");
                // B? qua metadata line
                byte[] remaining = new byte[dataBuffer.Length - metaEndIdx - 1];
                Buffer.BlockCopy(dataBuffer, metaEndIdx + 1, remaining, 0, remaining.Length);
                dataBuffer = remaining;
                return false;
            }

            string voiceId = parts[1];
            string senderId = parts[2];
            string senderName = parts[3];
            string recipientId = parts[4];
            int duration = int.Parse(parts[5]);
            string audioFileName = parts[6];
            string transcriptionFileName = parts[7];
            DateTime timestamp = DateTime.Parse(parts[8]);

            System.Diagnostics.Debug.WriteLine($"[VOICE] Parsed: ID={voiceId}, Audio={audioFileName}");

            // B? qua metadata line
            int currentPos = metaEndIdx + 1;

            System.Diagnostics.Debug.WriteLine($"[VOICE] currentPos after metadata: {currentPos}, remaining buffer: {dataBuffer.Length - currentPos}");

            // S? d?ng path đúng (bao g?m SenderId folder)
            string baseFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "VoiceChat",
                senderId
            );
            string voiceFolder = Path.Combine(baseFolder, "voice_messages");
            Directory.CreateDirectory(voiceFolder);

            System.Diagnostics.Debug.WriteLine($"[VOICE] Base folder with SenderId: {baseFolder}");
            System.Diagnostics.Debug.WriteLine($"[VOICE] Voice folder: {voiceFolder}");

            bool audioSaved = false;
            bool transcriptionSaved = false;

            // T?m VOICE_AUDIO header
            if (currentPos < dataBuffer.Length)
            {
                string audioHeaderText = Encoding.UTF8.GetString(dataBuffer, currentPos, Math.Min(100, dataBuffer.Length - currentPos));

                if (audioHeaderText.StartsWith("VOICE_AUDIO|"))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOICE] Found VOICE_AUDIO");

                    // T?m end of header (\n)
                    int audioHeaderEnd = -1;
                    for (int i = currentPos; i < dataBuffer.Length; i++)
                    {
                        if (dataBuffer[i] == '\n')
                        {
                            audioHeaderEnd = i;
                            break;
                        }
                    }

                    if (audioHeaderEnd < 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[VOICE] Audio header not complete - waiting for more data");
                        return true; // Đang xử lý voice message
                    }

                    string audioHeader = Encoding.UTF8.GetString(dataBuffer, currentPos, audioHeaderEnd - currentPos);
                    System.Diagnostics.Debug.WriteLine($"[VOICE] Audio header: {audioHeader}");
                    string[] audioHeaderParts = audioHeader.Split('|');

                    if (audioHeaderParts.Length >= 3 && int.TryParse(audioHeaderParts[2], out int audioSize))
                    {
                        System.Diagnostics.Debug.WriteLine($"[VOICE] Audio size from header: {audioSize}");

                        int audioDataStart = audioHeaderEnd + 1;
                        int audioDataEnd = audioDataStart + audioSize;

                        // T?m delimiter
                        byte[] delimiterPattern = Encoding.UTF8.GetBytes("VOICE_AUDIO_END\n");
                        int delimiterPos = FindPattern(dataBuffer, delimiterPattern, audioDataEnd);

                        if (delimiterPos < 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VOICE] Waiting for audio delimiter");
                            return true; // Đang xử lý voice message
                        }

                        // Lưu file .wav (raw bytes)
                        byte[] audioBytes = new byte[audioSize];
                        Buffer.BlockCopy(dataBuffer, audioDataStart, audioBytes, 0, audioSize);

                        string audioPath = Path.Combine(voiceFolder, audioFileName);
                        File.WriteAllBytes(audioPath, audioBytes);
                        System.Diagnostics.Debug.WriteLine($"[VOICE] ? Saved audio: {audioPath}, size: {audioSize}");

                        currentPos = delimiterPos + delimiterPattern.Length;
                        audioSaved = true;
                    }
                    else
                    {
                        System.Diagnostics.Debug.WriteLine($"[VOICE] Invalid audio header parts: {audioHeaderParts.Length}");
                    }
                }
            }
            else
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE] Not enough data after metadata - waiting");
                return true; // Đang xử lý voice message
            }

            // T?m VOICE_TRANSCRIPTION header
            if (currentPos < dataBuffer.Length)
            {
                string transcHeaderText = Encoding.UTF8.GetString(dataBuffer, currentPos, Math.Min(100, dataBuffer.Length - currentPos));
                if (transcHeaderText.StartsWith("VOICE_TRANSCRIPTION|"))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOICE] Found VOICE_TRANSCRIPTION");

                    // T?m end of header (\n)
                    int transcHeaderEnd = -1;
                    for (int i = currentPos; i < dataBuffer.Length; i++)
                    {
                        if (dataBuffer[i] == '\n')
                        {
                            transcHeaderEnd = i;
                            break;
                        }
                    }

                    if (transcHeaderEnd < 0)
                    {
                        System.Diagnostics.Debug.WriteLine($"[VOICE] Transcription header not complete - waiting");
                        return true; // Đang xử lý voice message
                    }

                    string transcHeader = Encoding.UTF8.GetString(dataBuffer, currentPos, transcHeaderEnd - currentPos);
                    string[] transcHeaderParts = transcHeader.Split('|');

                    if (transcHeaderParts.Length >= 3 && int.TryParse(transcHeaderParts[2], out int transcSize))
                    {
                        System.Diagnostics.Debug.WriteLine($"[VOICE] Transcription size: {transcSize}");

                        int transcDataStart = transcHeaderEnd + 1;
                        int transcDataEnd = transcDataStart + transcSize;

                        // T?m delimiter
                        byte[] delimiterPattern = Encoding.UTF8.GetBytes("VOICE_TRANSCRIPTION_END\n");
                        int delimiterPos = FindPattern(dataBuffer, delimiterPattern, transcDataEnd);

                        if (delimiterPos < 0)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VOICE] Waiting for transcription delimiter");
                            return true; // Đang xử lý voice message
                        }

                        // Lưu file .txt
                        string transcContent = Encoding.UTF8.GetString(dataBuffer, transcDataStart, transcSize);
                        string transcPath = Path.Combine(voiceFolder, transcriptionFileName);
                        File.WriteAllText(transcPath, transcContent, Encoding.UTF8);
                        System.Diagnostics.Debug.WriteLine($"[VOICE] ? Saved transcription: {transcPath}");

                        currentPos = delimiterPos + delimiterPattern.Length;
                        transcriptionSaved = true;
                    }
                }
            }

            // Chỉ trigger event nếu cả 2 file đã được lưu
            if (audioSaved && transcriptionSaved)
            {
                // T?o VoiceMessage object
                VoiceMessage voiceMessage = new VoiceMessage
                {
                    Id = voiceId,
                    SenderId = senderId,
                    SenderName = senderName,
                    RecipientId = recipientId,
                    AudioFileName = audioFileName,
                    TranscriptionFileName = transcriptionFileName,
                    Duration = duration,
                    Timestamp = timestamp,
                    UserDataPath = baseFolder,
                    IsOwn = false
                };

                System.Diagnostics.Debug.WriteLine($"[VOICE] ? Invoking VoiceMessageReceived event");
                VoiceMessageReceived?.Invoke(this, voiceMessage);

                // C?p nh?t buffer - b? qua các d? li?u đ? x? l?
                if (currentPos < dataBuffer.Length)
                {
                    byte[] remaining = new byte[dataBuffer.Length - currentPos];
                    Buffer.BlockCopy(dataBuffer, currentPos, remaining, 0, remaining.Length);
                    dataBuffer = remaining;
                }
                else
                {
                    dataBuffer = new byte[0];
                }

                return true; // Đã xử lý voice message xong
            }

            return true; // Đang xử lý voice message
        }

        // Helper method để tìm pattern trong byte array
        private int FindPattern(byte[] source, byte[] pattern, int startIndex)
        {
            if (startIndex + pattern.Length > source.Length)
                return -1;

            for (int i = startIndex; i <= source.Length - pattern.Length; i++)
            {
                bool found = true;
                for (int j = 0; j < pattern.Length; j++)
                {
                    if (source[i + j] != pattern[j])
                    {
                        found = false;
                        break;
                    }
                }
                if (found)
                    return i;
            }
            return -1;
        }

        public void Disconnect()
        {
            try
            {
                networkStream?.Close();
                tcpClient?.Close();
            }
            catch { }
        }
    }
}