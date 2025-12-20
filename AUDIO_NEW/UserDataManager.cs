using Newtonsoft.Json;

namespace AUDIO_NEW
{
    /// <summary>
    /// Quản lý lưu trữ và đọc dữ liệu người dùng
    /// </summary>
    public class UserDataManager
    {
        private string appDataPath;
        private string userDataPath;
        private string userName;

        public string DisplayName { get; private set; }
        public string UserGender { get; private set; }

        public UserDataManager(string userName, string displayName = "", string gender = "Khác")
        {
            this.userName = userName;
            this.DisplayName = string.IsNullOrEmpty(displayName) ? userName : displayName;
            this.UserGender = gender;

            // Khởi tạo đường dẫn lưu trữ
            appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "VoiceChat");
            userDataPath = Path.Combine(appDataPath, userName);

            // Tạo thư mục nếu chưa tồn tại
            Directory.CreateDirectory(appDataPath);
            Directory.CreateDirectory(userDataPath);

            LoadUserData();
        }

        /// <summary>
        /// Lưu dữ liệu người dùng vào file JSON
        /// </summary>
        public void SaveUserData()
        {
            try
            {
                var userData = new
                {
                    Username = userName,
                    DisplayName = DisplayName,
                    Gender = UserGender,
                    LastUpdated = DateTime.Now
                };

                string filePath = Path.Combine(userDataPath, "profile.json");
                string json = JsonConvert.SerializeObject(userData, Formatting.Indented);
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lưu dữ liệu: " + ex.Message);
            }
        }

        /// <summary>
        /// Load dữ liệu người dùng từ file JSON
        /// </summary>
        private void LoadUserData()
        {
            try
            {
                string filePath = Path.Combine(userDataPath, "profile.json");
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    dynamic userData = JsonConvert.DeserializeObject(json);
                    if (string.IsNullOrEmpty(DisplayName) || DisplayName == userName)
                    {
                        DisplayName = userData.DisplayName ?? userName;
                    }
                    else
                    {
                        SaveUserData();
                    }
                }
                else
                {
                    SaveUserData();
                }
            }
            catch
            {
                if (string.IsNullOrEmpty(DisplayName))
                {
                    DisplayName = userName;
                }
            }
        }

        /// <summary>
        /// Lưu lịch sử cuộc gọi
        /// </summary>
        public void SaveCallHistory(string targetUser, string targetDisplayName, string callType, TimeSpan duration)
        {
            try
            {
                var callRecord = new
                {
                    TargetUsername = targetUser,
                    TargetDisplayName = targetDisplayName,
                    CallType = callType, // "outgoing" hoặc "incoming"
                    Duration = duration.ToString(@"hh\:mm\:ss"),
                    Timestamp = DateTime.Now
                };

                string historyDir = Path.Combine(userDataPath, "history");
                Directory.CreateDirectory(historyDir);

                string filePath = Path.Combine(historyDir, "calls.json");
                List<dynamic> callHistory = new List<dynamic>();

                // Load lịch sử cũ nếu tồn tại
                if (File.Exists(filePath))
                {
                    string json = File.ReadAllText(filePath);
                    callHistory = JsonConvert.DeserializeObject<List<dynamic>>(json) ?? new List<dynamic>();
                }

                callHistory.Add(callRecord);

                // Lưu lại
                string updatedJson = JsonConvert.SerializeObject(callHistory, Formatting.Indented);
                File.WriteAllText(filePath, updatedJson);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi lưu lịch sử: " + ex.Message);
            }
        }

        public string GetUserDataPath()
        {
            return userDataPath;
        }
    }
}