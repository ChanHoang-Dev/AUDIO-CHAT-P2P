using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AUDIO_NEW
{
    public partial class FormInfor : Form
    {
        private UserDataManager userDataManager;

        public FormInfor()
        {
            InitializeComponent();

            // Setup gender combo box
            List<string> genders = new List<string> { "Nam", "Nữ", "Khác" };
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(genders.ToArray());
            comboBox1.SelectedIndex = 0;
        }

        private void FormInfor_Load(object sender, EventArgs e)
        {
            // Set default display name to machine name
            txtDisplayname.Text = Environment.MachineName;

            // Try to load existing profile
            string username = Environment.MachineName;
            userDataManager = new UserDataManager(username);

            // If profile exists, pre-fill the form
            if (!string.IsNullOrEmpty(userDataManager.DisplayName))
            {
                txtDisplayname.Text = userDataManager.DisplayName;

                // Set gender if exists
                int genderIndex = comboBox1.Items.IndexOf(userDataManager.UserGender);
                if (genderIndex >= 0)
                {
                    comboBox1.SelectedIndex = genderIndex;
                }
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            string displayName = txtDisplayname.Text.Trim();
            string gender = comboBox1.Text.Trim();

            if (string.IsNullOrWhiteSpace(displayName))
            {
                MessageBox.Show("Vui lòng nhập tên hiển thị!", "Thông báo", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtDisplayname.Focus();
                return;
            }

            try
            {
                // Create/Update UserDataManager
                string username = Environment.MachineName;
                userDataManager = new UserDataManager(username, displayName, gender);
                userDataManager.SaveUserData();

                MessageBox.Show("Lưu profile thành công!", "Thành công", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Open MainForm
                MainForm mainForm = new MainForm(username, displayName, gender);
                mainForm.Show();
                this.Hide();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message, "Lỗi", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}