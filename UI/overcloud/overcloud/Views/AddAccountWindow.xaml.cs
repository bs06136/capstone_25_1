using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Service;
using overcloud.Models;  // CloudAccountInfo 클래스 사용을 위해 추가
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {
        private readonly AccountService _accountService;

        public AddAccountWindow(AccountService service)
        {
            InitializeComponent();
            _accountService = service;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string id = txtID.Text;
            string password = txtPassword.Password;
            string cloudType = (cloudComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            CloudAccountInfo accountInfo = new CloudAccountInfo
            {
                ID = id,
                Password = password,
                CloudType = cloudType,
                TotalSize = 0,
                UsedSize = 0
            };

            bool result = _accountService.AddAccount(accountInfo);
            MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            this.Close();
        }
    }
}
