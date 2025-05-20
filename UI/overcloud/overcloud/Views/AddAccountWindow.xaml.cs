using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Repository;
using DB.overcloud.Models;  // CloudAccountInfo 클래스 사용을 위해 추가
//using static overcloud.temp_class.TempClass;  // CloudAccountInfo 클래스 사용을 위해 추가
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {
        private AccountService _accountService;     //수정 필요
        private string _user_id;                    //수정 필요

        public AddAccountWindow(AccountService accountService, string user_id)
        {

            InitializeComponent();
            _accountService = accountService;
            _user_id = user_id;                   //수정 필요

        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Confirm_Click call");
            string id = txtID.Text;
            string password = txtPassword.Password;
            string cloudType = (cloudComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            CloudStorageInfo accountInfo = new CloudStorageInfo
            {
                AccountId = id,
                AccountPassword = password,
                CloudType = cloudType,
				TotalCapacity = 0,
				UsedCapacity = 0
            };

            System.Diagnostics.Debug.WriteLine(cloudType);
            // ⭐ 객체 생성 없이 정적 메서드 직접 호출
            bool result = await _accountService.Add_Cloud_Storage(accountInfo, _user_id);
            System.Windows.MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            this.Close();
        }
    }
}
