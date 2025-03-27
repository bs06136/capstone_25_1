using System.Windows;
using System.Collections.Generic;
using overcloud.Models;

namespace overcloud.Views
{
    public partial class DeleteAccountWindow : Window
    {
        public DeleteAccountWindow()
        {
            InitializeComponent();
            LoadAccounts();
        }

        // 연결된 계정 목록 불러오기
        private void LoadAccounts()
        {
            var main = System.Windows.Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                List<CloudStorageInfo> accounts = main.GetAllCloudStatus();
                AccountListBox.ItemsSource = accounts;
            }
        }

        private void ConfirmDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedAccount = AccountListBox.SelectedItem as CloudStorageInfo;

            if (selectedAccount == null)
            {
                System.Windows.MessageBox.Show("삭제할 계정을 선택해 주세요.");
                return;
            }

            // 임의로 ID, password를 입력받거나 고정값을 사용
            string id = "testUser"; // 실제 구현에선 사용자로부터 입력받아야 함
            string password = "testPass"; // 실제 구현에선 사용자로부터 입력받아야 함

            var main = System.Windows.Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                bool result = main.ID_del(id, password, selectedAccount.ServiceName);
                System.Windows.MessageBox.Show(result ? "계정 삭제 성공" : "계정 삭제 실패");
            }

            this.Close();
        }
    }
}
