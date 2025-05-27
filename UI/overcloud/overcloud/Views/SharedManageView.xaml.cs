using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using DB.overcloud.Repository;
using overcloud.Views;

namespace overcloud.Views
{
    public partial class SharedManageView : System.Windows.Controls.UserControl
    {
        private LoginController _controller;
        private string _user_id;

        public SharedManageView(LoginController controller, string user_id)
        {
            InitializeComponent();
            _controller = controller;
            _user_id = user_id;

            // 최초 로드 시 “공유 계정” 목록 화면으로
            SubFrame.Navigate(new SharedAccountListView(_controller, _user_id));
        }

        private void SharedManageMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new SharedAccountListView(_controller, _user_id));
        }

        private void SharedDetailMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new SharedAccountDetailView(_controller, _user_id));
        }
    }
}
