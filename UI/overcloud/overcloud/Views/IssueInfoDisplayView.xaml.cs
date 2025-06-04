using System.Windows.Controls;
using DB.overcloud.Models;

namespace overcloud.Views
{
    public partial class IssueInfoDisplayView : System.Windows.Controls.UserControl
    {
        private readonly FileIssueInfo _issueInfo;

        public IssueInfoDisplayView(FileIssueInfo issueInfo)
        {
            InitializeComponent();
            _issueInfo = issueInfo;
            LoadData();
        }

        private void LoadData()
        {
            TitleTextBox.Text = _issueInfo.Title;
            DescriptionTextBox.Text = _issueInfo.Description;
            AssignedToTextBox.Text = _issueInfo.AssignedTo ?? "";
            StatusTextBox.Text = _issueInfo.Status;
            DueDateTextBox.Text = _issueInfo.DueDate?.ToString("yyyy-MM-dd") ?? "";
        }
    }
}
