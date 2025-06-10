using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace overcloud.Views
{
    public partial class FileSearchView : System.Windows.Controls.UserControl
    {
        // 검색어를 부모로 전달하는 이벤트
        public event Action<string> SearchSubmitted;

        public FileSearchView()
        {
            InitializeComponent();
        }

        private void OnSearchButtonClick(object sender, RoutedEventArgs e)
        {
            SubmitSearch();
        }

        private void SearchTextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SubmitSearch();
        }

        private void SubmitSearch()
        {
            var query = SearchTextBox.Text.Trim();
            if (!string.IsNullOrEmpty(query))
                SearchSubmitted?.Invoke(query);
        }
    }
}
