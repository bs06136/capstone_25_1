using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace overcloud.Views
{
    public partial class AccountView : System.Windows.Controls.UserControl
    {
        public AccountView()
        {
            InitializeComponent();
            // 최초 로드 시 “계정 관리” 목록 화면으로
            SubFrame.Navigate(new AccountListView());
        }

        private void AccountMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountListView());
        }

        private void DetailMenu_Click(object sender, MouseButtonEventArgs e)
        {
            SubFrame.Navigate(new AccountDetailView());
        }
    }
}
