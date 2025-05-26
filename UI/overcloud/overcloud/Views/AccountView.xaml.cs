using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using OverCloud.Services.FileManager;
using OverCloud.Services.StorageManager;
using OverCloud.Services;
using DB.overcloud.Repository;

namespace overcloud.Views
{
    public partial class AccountView : System.Windows.Controls.UserControl
    {
        // 서비스 초기화

        private LoginController _controller;
        private string _user_id;

        public AccountView(LoginController controller, string user_id)
        {
            InitializeComponent();
            _controller = controller;
            _user_id = user_id;

            // 최초 로드 시 “계정 관리” 목록 화면으로
            SubFrame.Navigate(new AccountListView(_controller, _user_id));
        }

        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountListView(_controller, _user_id));
        }

        private void DetailMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountDetailView(_controller, _user_id));
        }
    }
}
