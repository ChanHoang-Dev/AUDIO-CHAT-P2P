using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;
using Vosk;
using Newtonsoft.Json.Linq;

namespace AUDIO_NEW
{
    /// <summary>
    /// Thông tin v? m?t voice message
    /// </summary>
    public class VoiceMessage
    {
        public string Id { get; set; }
        public string SenderId { get; set; }
        public string SenderName { get; set; }
        public string RecipientId { get; set; }
        public string AudioFileName { get; set; }
        public string TranscriptionFileName { get; set; }
        public int Duration { get; set; }
        public DateTime Timestamp { get; set; }
        public bool IsOwn { get; set; }
        public string UserDataPath { get; set; }

        public VoiceMessage()
        {
            Id = Guid.NewGuid().ToString();
            Timestamp = DateTime.Now;
        }

        public string GetAudioPath(string baseFolder)
        {
            return Path.Combine(baseFolder, "voice_messages", AudioFileName);
        }

        public string GetTranscriptionPath(string baseFolder)
        {
            return Path.Combine(baseFolder, "voice_messages", TranscriptionFileName);
        }
    }

    /// <summary>
    /// Qu?n l? ghi âm, lưu file và Speech-to-Text v?i Vosk
    /// </summary>
    public class VoiceMessageManager
    {
        private WaveInEvent waveIn;
        private WaveFileWriter waveWriter;
        private string recordingPath;
        private bool isRecording = false;
        private static Model voskModel;
        private static readonly object modelLock = new object();

        public event EventHandler<VoiceMessage> VoiceMessageSaved;

        public VoiceMessageManager(string userDataPath)
        {
            recordingPath = Path.Combine(userDataPath, "voice_messages");
            Directory.CreateDirectory(recordingPath);

            // Khởi tạo Vosk model (chỉ 1 lần)
            InitializeVoskModel();
        }

        /// <summary>
        /// Khởi tạo Vosk model (chỉ load 1 lần)
        /// </summary>
        private void InitializeVoskModel()
        {
            if (voskModel != null) return;

            lock (modelLock)
            {
                if (voskModel != null) return;

                try
                {
                    // Tìm model path
                    string modelPath = FindVoskModelPath();

                    if (string.IsNullOrEmpty(modelPath))
                    {
                        System.Diagnostics.Debug.WriteLine("[VOSK] Model not found! Please download vosk-model-small-vi-0.4");
                        return;
                    }

                    System.Diagnostics.Debug.WriteLine($"[VOSK] Loading model from: {modelPath}");

                    // Tắt log của Vosk (để console sạch hơn)
                    Vosk.Vosk.SetLogLevel(-1);

                    voskModel = new Model(modelPath);
                    System.Diagnostics.Debug.WriteLine("[VOSK] Model loaded successfully!");
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[VOSK ERROR] Failed to load model: {ex.Message}");
                    voskModel = null;
                }
            }
        }

        /// <summary>
        /// Tìm đường dẫn Vosk model
        /// </summary>
        private string FindVoskModelPath()
        {
            // Thử các vị trí có thể có
            string[] possiblePaths = new[]
            {
                // Trong thư mục project
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "vosk-model-small-vn-0.4"),
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "models", "vosk-model-small-vn-0.4"),
                
                // Trong thư mục solution
                Path.Combine(Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.FullName, "vosk-model-small-vn-0.4"),
                
                // Trong thư mục user data
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoiceChat", "models", "vosk-model-small-vn-0.4"),
            };

            foreach (string path in possiblePaths)
            {
                if (Directory.Exists(path))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOSK] Found model at: {path}");
                    return path;
                }
            }

            System.Diagnostics.Debug.WriteLine("[VOSK] Model not found in any location");
            return null;
        }

        /// <summary>
        /// B?t đ?u ghi âm
        /// </summary>
        public void StartRecording()
        {
            try
            {
                if (isRecording) return;

                waveIn = new WaveInEvent
                {
                    WaveFormat = new WaveFormat(16000, 16, 1) // 16kHz, 16-bit, mono (yêu cầu của Vosk)
                };

                string tempFile = Path.Combine(recordingPath, $"temp_recording_{Guid.NewGuid()}.wav");
                waveWriter = new WaveFileWriter(tempFile, waveIn.WaveFormat);

                waveIn.DataAvailable += (s, e) =>
                {
                    waveWriter.Write(e.Buffer, 0, e.BytesRecorded);
                };

                waveIn.StartRecording();
                isRecording = true;
                System.Diagnostics.Debug.WriteLine("[VOICE] Recording started");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Start recording: {ex.Message}");
            }
        }

        /// <summary>
        /// D?ng ghi âm
        /// </summary>
        public string StopRecording()
        {
            try
            {
                if (!isRecording) return null;

                waveIn?.StopRecording();
                waveWriter?.Flush();
                waveWriter?.Dispose();

                string tempFile = null;
                var files = Directory.GetFiles(recordingPath, "temp_recording_*.wav");
                if (files.Length > 0)
                {
                    tempFile = files[files.Length - 1];
                }

                isRecording = false;
                System.Diagnostics.Debug.WriteLine($"[VOICE] Recording stopped: {tempFile}");
                return tempFile;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Stop recording: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Lưu voice message
        /// </summary>
        public async Task<VoiceMessage> SaveVoiceMessage(
            string senderId,
            string senderName,
            string recipientId,
            int durationSeconds,
            string audioFilePath,
            string transcription = "")
        {
            try
            {
                if (!File.Exists(audioFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Audio file not found: {audioFilePath}");
                    return null;
                }

                var voiceMsg = new VoiceMessage
                {
                    SenderId = senderId,
                    SenderName = senderName,
                    RecipientId = recipientId,
                    Duration = durationSeconds,
                    UserDataPath = Path.GetDirectoryName(recordingPath)
                };

                voiceMsg.AudioFileName = $"{voiceMsg.Id}.wav";
                voiceMsg.TranscriptionFileName = $"{voiceMsg.Id}.txt";

                string finalAudioPath = voiceMsg.GetAudioPath(Path.GetDirectoryName(recordingPath));
                File.Copy(audioFilePath, finalAudioPath, true);
                System.Diagnostics.Debug.WriteLine($"[VOICE] Audio saved: {finalAudioPath}");

                string transcriptionPath = voiceMsg.GetTranscriptionPath(Path.GetDirectoryName(recordingPath));
                await File.WriteAllTextAsync(transcriptionPath, transcription, Encoding.UTF8);
                System.Diagnostics.Debug.WriteLine($"[VOICE] Transcription saved: {transcriptionPath}");

                try { File.Delete(audioFilePath); } catch { }

                return voiceMsg;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Save voice message: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Chuy?n đ?i âm thanh thành text v?i Vosk
        /// </summary>
        public async Task<string> TranscribeAudioAsync(string audioFilePath)
        {
            return await Task.Run(() =>
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] Starting: {audioFilePath}");

                    if (!File.Exists(audioFilePath))
                    {
                        return "❌ File không tồn tại";
                    }

                    if (voskModel == null)
                    {
                        System.Diagnostics.Debug.WriteLine("[VOSK TRANSCRIBE] Model not loaded!");

                        // Fallback to simple info
                        FileInfo fi = new FileInfo(audioFilePath);
                        return $"🎤 Tin nhắn thoại\n📊 {fi.Length / 1024}KB\n⚠️ Vosk model chưa được cài đặt\n💡 Hãy download vosk-model-small-vi-0.4";
                    }

                    FileInfo fileInfo = new FileInfo(audioFilePath);
                    System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] File size: {fileInfo.Length} bytes");

                    // Đọc và xử lý audio file
                    using (var waveReader = new WaveFileReader(audioFilePath))
                    {
                        // Kiểm tra format (Vosk cần 16kHz, mono)
                        if (waveReader.WaveFormat.SampleRate != 16000 || waveReader.WaveFormat.Channels != 1)
                        {
                            System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] Converting audio format...");
                            audioFilePath = ConvertAudioFormat(audioFilePath);
                        }
                    }

                    // Tạo recognizer
                    using (var recognizer = new VoskRecognizer(voskModel, 16000.0f))
                    {
                        recognizer.SetMaxAlternatives(0);
                        recognizer.SetWords(false);

                        StringBuilder transcription = new StringBuilder();

                        using (var audioStream = File.OpenRead(audioFilePath))
                        {
                            // Skip WAV header (44 bytes)
                            audioStream.Seek(44, SeekOrigin.Begin);

                            byte[] buffer = new byte[4096];
                            int bytesRead;
                            int totalProcessed = 0;

                            while ((bytesRead = audioStream.Read(buffer, 0, buffer.Length)) > 0)
                            {
                                totalProcessed += bytesRead;

                                if (recognizer.AcceptWaveform(buffer, bytesRead))
                                {
                                    string result = recognizer.Result();
                                    var jsonResult = JObject.Parse(result);
                                    string text = jsonResult["text"]?.ToString();

                                    if (!string.IsNullOrWhiteSpace(text))
                                    {
                                        transcription.Append(text + " ");
                                        System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] Partial: {text}");
                                    }
                                }
                            }

                            // Lấy kết quả cuối cùng
                            string finalResult = recognizer.FinalResult();
                            var finalJson = JObject.Parse(finalResult);
                            string finalText = finalJson["text"]?.ToString();

                            if (!string.IsNullOrWhiteSpace(finalText))
                            {
                                transcription.Append(finalText);
                                System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] Final: {finalText}");
                            }

                            System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] Processed {totalProcessed} bytes");
                        }

                        string fullTranscription = transcription.ToString().Trim();

                        if (string.IsNullOrEmpty(fullTranscription))
                        {
                            System.Diagnostics.Debug.WriteLine("[VOSK TRANSCRIBE] No speech detected");

                            using (var reader = new WaveFileReader(audioFilePath))
                            {
                                double duration = reader.TotalTime.TotalSeconds;
                                return $"🎤 Tin nhắn thoại - {duration:F0}s\n📊 {fileInfo.Length / 1024}KB\n🔇 Không phát hiện giọng nói";
                            }
                        }

                        System.Diagnostics.Debug.WriteLine($"[VOSK TRANSCRIBE] ✓ SUCCESS: {fullTranscription}");

                        // Format kết quả đẹp
                        return $"🎤 {fullTranscription}\n\n📊 {fileInfo.Length / 1024}KB • {DateTime.Now:HH:mm}";
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[VOSK ERROR] Transcription failed: {ex.Message}");
                    System.Diagnostics.Debug.WriteLine($"[VOSK ERROR] Stack: {ex.StackTrace}");

                    // Fallback
                    try
                    {
                        FileInfo fileInfo = new FileInfo(audioFilePath);
                        using (var reader = new WaveFileReader(audioFilePath))
                        {
                            double duration = reader.TotalTime.TotalSeconds;
                            return $"🎤 Tin nhắn thoại - {duration:F0}s\n📊 {fileInfo.Length / 1024}KB\n⚠️ Lỗi: {ex.Message}";
                        }
                    }
                    catch
                    {
                        return $"❌ Lỗi: {ex.Message}";
                    }
                }
            });
        }

        /// <summary>
        /// Convert audio về format mà Vosk yêu cầu (16kHz, mono, 16-bit PCM)
        /// </summary>
        private string ConvertAudioFormat(string inputPath)
        {
            try
            {
                string outputPath = Path.Combine(Path.GetDirectoryName(inputPath),
                    $"converted_{Path.GetFileName(inputPath)}");

                using (var reader = new WaveFileReader(inputPath))
                {
                    var outFormat = new WaveFormat(16000, 16, 1); // 16kHz, 16-bit, mono

                    using (var resampler = new MediaFoundationResampler(reader, outFormat))
                    {
                        WaveFileWriter.CreateWaveFile(outputPath, resampler);
                    }
                }

                System.Diagnostics.Debug.WriteLine($"[VOSK] Converted audio to: {outputPath}");
                return outputPath;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VOSK ERROR] Audio conversion failed: {ex.Message}");
                return inputPath; // Return original if conversion fails
            }
        }

        /// <summary>
        /// Phát âm thanh
        /// </summary>
        public void PlayAudio(string audioFilePath)
        {
            try
            {
                if (!File.Exists(audioFilePath))
                {
                    System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Audio file not found: {audioFilePath}");
                    return;
                }

                var waveOutEvent = new WaveOutEvent();
                var audioFileReader = new AudioFileReader(audioFilePath);
                waveOutEvent.Init(audioFileReader);
                waveOutEvent.Play();

                System.Diagnostics.Debug.WriteLine($"[VOICE] Playing: {audioFilePath}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[VOICE ERROR] Play audio: {ex.Message}");
            }
        }

        /// <summary>
        /// Cleanup
        /// </summary>
        public void Cleanup()
        {
            try
            {
                waveIn?.StopRecording();
                waveIn?.Dispose();
                waveWriter?.Dispose();
            }
            catch { }
        }
    }
}