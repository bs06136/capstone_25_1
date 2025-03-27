using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;

namespace overcloud.Views
{
    public partial class AddAccountWindow : Window
    {
        public AddAccountWindow()
        {
            InitializeComponent();
        }

        // ⭐ 누락된 메서드 추가
        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            string id = txtID.Text;
            string password = txtPassword.Password; // PasswordBox는 .Password로 접근

            string cloudName = "Google Drive"; // 임시로 고정값 사용중

            // MainWindow의 ID_add 호출
            var main = System.Windows.Application.Current.MainWindow as MainWindow;
            if (main != null)
            {
                bool result = main.ID_add(id, password, cloudName);
                System.Windows.MessageBox.Show(result ? "계정 추가 성공" : "계정 추가 실패");
            }

            this.Close(); // 창 닫기
        }
    }
}
