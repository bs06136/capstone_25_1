using OverCloud.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace overcloud.Views
{
    public partial class FileSearchView : System.Windows.Controls.UserControl
    {
        private LoginController _controller;
        private string _user_id;

        public FileSearchView(LoginController controller, string user_id)
        {
            InitializeComponent();
            _controller = controller;
            _user_id = user_id;
        }

        private void SearchButton_Click(object sender, RoutedEventArgs e)
        {
            PerformSearch();
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                PerformSearch();
            }
        }

        private void PerformSearch()
        {
            string keyword = SearchTextBox.Text.Trim();

            if (!string.IsNullOrEmpty(keyword))
            {
                // 예시: 검색 결과 생성
                var results = _controller.FileRepository.FindByFileName(keyword, _user_id);

                // 경로 포함시켜서 바인딩용 데이터 생성
                var viewModels = results.Select(f => new
                {
                    FileName = f.FileName,
                    FullPath = _controller.FileRepository.GetFullPath(f.FileId)
                }).ToList();

                SearchResultList.ItemsSource = viewModels;
            }
        }
    }
}
