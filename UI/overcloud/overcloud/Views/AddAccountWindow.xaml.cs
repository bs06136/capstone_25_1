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
        private LoginController _controller; 
        private string _user_id;                    //수정 필요
        bool _isCooperationMode; 

        public AddAccountWindow(LoginController controller, string user_id ,  bool coop)
        {

            InitializeComponent();
            _controller = controller;
            _user_id = user_id;
            _isCooperationMode = coop;

        }

        private void AddAccountWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isCooperationMode)
            {
                // 협업 계정 리스트 보여주기
                cooperationComboBox.Visibility = Visibility.Visible;
                cooperationComboBox.Items.Clear();
                cooperationComboBox.Items.Add(new ComboBoxItem { Content = "test@example.com" });

                List<string> cooperationAccounts = _controller.CoopUserRepository.connected_cooperation_account_nums(_user_id);
                Console.WriteLine("협업 계정 수: " + cooperationAccounts.Count);
                cooperationComboBox.Items.Clear();
                foreach (var acc in cooperationAccounts)
                {
                    cooperationComboBox.Items.Add(new ComboBoxItem { Content = acc });
                }

                if (cooperationComboBox.Items.Count > 0)
                {
                    (cooperationComboBox.Items[0] as ComboBoxItem).IsSelected = true;
                }
            }
        }

        private async void Confirm_Click(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("Confirm_Click call");
            string id = txtID.Text;
            string password = txtPassword.Password;
            string cloudType = (cloudComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            string cooperationTargetId = null;
            if (_isCooperationMode && cooperationComboBox.SelectedItem is ComboBoxItem item)
            {
                cooperationTargetId = item.Content.ToString();
            }
            else
            {
                // 협업 모드가 아니거나 선택된 항목이 없을 때
                cooperationTargetId = _user_id;  // 현재 사용자 ID로 설정
            }

                CloudStorageInfo accountInfo = new CloudStorageInfo
                {
                    ID = cooperationTargetId,
                    AccountId = id,
                    AccountPassword = password,
                    CloudType = cloudType,
                    TotalCapacity = 0,
                    UsedCapacity = 0
                };

            bool result;
            result = await _controller.AccountService.Add_Cloud_Storage(accountInfo, _user_id);

            System.Diagnostics.Debug.WriteLine(cloudType);
            // ⭐ 객체 생성 없이 정적 메서드 직접 호출
            System.Windows.MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            this.Close();
        }
    }
}
