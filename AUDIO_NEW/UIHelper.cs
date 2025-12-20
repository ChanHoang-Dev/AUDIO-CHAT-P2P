using System.Drawing;

namespace AUDIO_NEW
{
    /// <summary>
    /// Helper class cho các chức năng UI
    /// </summary>
    public static class UIHelper
    {
        /// <summary>
        /// Resize image với chất lượng cao
        /// </summary>
        public static Image ResizeImage(Image img, int width, int height)
        {
            Bitmap b = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(b))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                g.DrawImage(img, 0, 0, width, height);
            }
            return b;
        }

        /// <summary>
        /// Lấy avatar dựa trên giới tính
        /// </summary>
        public static Image GetAvatarByGender(string gender, int width = 50, int height = 50)
        {
            try
            {
                Image avatar = gender switch
                {
                    "Nam" => Properties.Resources.Male,
                    "Nữ" => Properties.Resources.Female,
                    _ => Properties.Resources.None
                };
                return ResizeImage(avatar, width, height);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Cập nhật danh sách peers trong ListView
        /// </summary>
        public static void UpdatePeerListView(ListView listView, List<PeerInfo> peers)
        {
            if (listView.InvokeRequired)
            {
                listView.Invoke(new Action(() => UpdatePeerListView(listView, peers)));
                return;
            }

            // Xóa những item không còn trong peers
            var itemsToRemove = new List<ListViewItem>();
            foreach (ListViewItem item in listView.Items)
            {
                string username = item.Tag?.ToString();
                if (string.IsNullOrEmpty(username) || !peers.Any(p => p.Username == username))
                {
                    itemsToRemove.Add(item);
                }
            }

            foreach (var item in itemsToRemove)
            {
                listView.Items.Remove(item);
            }

            // Cập nhật hoặc thêm peer
            foreach (var peer in peers)
            {
                var existingItem = listView.Items.OfType<ListViewItem>()
                    .FirstOrDefault(item => item.Tag?.ToString() == peer.Username);

                if (existingItem != null)
                {
                    // Cập nhật item hiện có
                    existingItem.SubItems[0].Text = peer.DisplayName;
                    existingItem.SubItems[1].Text = " (Online)";
                    existingItem.SubItems[2].Text = peer.EndPoint.Address.ToString();
                    existingItem.ForeColor = Color.Green;
                }
                else
                {
                    // Thêm item mới
                    var newItem = new ListViewItem(peer.DisplayName);
                    newItem.SubItems.Add(" (Online)");
                    newItem.SubItems.Add(peer.EndPoint.Address.ToString());
                    newItem.ForeColor = Color.Green;
                    newItem.Tag = peer.Username;
                    listView.Items.Add(newItem);
                }
            }
        }
    }
}