using System.Windows;
using DB.overcloud.Models;
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class IssueDetailWindow : Window
    {
        private readonly LoginController _controller;
        private readonly FileIssueInfo _issueInfo;

        public IssueDetailWindow(LoginController controller, FileIssueInfo issueInfo)
        {
            InitializeComponent();
            _controller = controller;
            _issueInfo = issueInfo;

            MainContentArea.Content = new IssueDetailView(_controller, _issueInfo, this);
        }

        public void SwitchRightDetail(System.Windows.Controls.UserControl rightContent)
        {
            (MainContentArea.Content as IssueDetailView)?.SwitchRight(rightContent);
        }
    }
}
