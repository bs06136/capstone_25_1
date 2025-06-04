using System;
using System.Collections.Generic;
using System.Windows;

namespace overcloud.Windows
{
    public partial class AddIssueDialog : Window
    {
        public string IssueTitle { get; private set; }
        public string IssueDescription { get; private set; }
        public string AssignedTo { get; private set; }
        public DateTime? DueDate { get; private set; }

        public AddIssueDialog(List<string> userList)
        {
            InitializeComponent();

            // 콤보박스 초기화
            AssigneeComboBox.Items.Add("");  // 담당자 없음 선택 가능
            foreach (var user in userList)
            {
                AssigneeComboBox.Items.Add(user);
            }

            AssigneeComboBox.SelectedIndex = 0;
            DueDatePicker.SelectedDate = null;
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            IssueTitle = TitleTextBox.Text.Trim();
            IssueDescription = DescriptionTextBox.Text.Trim();
            AssignedTo = AssigneeComboBox.SelectedItem?.ToString();
            DueDate = DueDatePicker.SelectedDate;

            if (string.IsNullOrEmpty(IssueTitle))
            {
                System.Windows.MessageBox.Show("제목을 입력해주세요.");
                return;
            }

            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
}
