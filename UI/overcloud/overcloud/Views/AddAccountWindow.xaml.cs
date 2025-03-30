using System.Windows;
using System.Windows.Controls;
using overcloud.Models;  // CloudAccountInfo 클래스 사용을 위해 추가
using static overcloud.temp_class.TempClass;  // CloudAccountInfo 클래스 사용을 위해 추가

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {

        public AddAccountWindow()
        {
            InitializeComponent();
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

            // ⭐ 객체 생성 없이 정적 메서드 직접 호출
            bool result = AddAccount(accountInfo);
            System.Windows.MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");

            this.Close();
        }
    }
}
