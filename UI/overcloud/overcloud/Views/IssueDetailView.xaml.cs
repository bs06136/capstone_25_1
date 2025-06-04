using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services;

namespace overcloud.Views
{
    public partial class IssueDetailView : System.Windows.Controls.UserControl
    {
        private readonly LoginController _controller;
        private readonly FileIssueInfo _issueInfo;
        private readonly IssueDetailWindow _parentWindow;

        public IssueDetailView(LoginController controller, FileIssueInfo issueInfo, IssueDetailWindow parentWindow)
        {
            InitializeComponent();

            _controller = controller;
            _issueInfo = issueInfo;
            _parentWindow = parentWindow;

            LoadRelatedFiles();
            LoadComments();
            LoadIssueDisplayView();
        }

        // 오른쪽 정보 디스플레이 로드
        private void LoadIssueDisplayView()
        {
            RightDetailArea.Content = new IssueInfoDisplayView(_issueInfo);
        }

        // 외부에서 호출하는 오른쪽 새로고침 (수정 후 복귀시 사용)
        public void ReloadRightDetail()
        {
            LoadIssueDisplayView();
        }

        private void LoadRelatedFiles()
        {
            var fileIds = _controller.FileIssueMappingRepository.GetFileIdsByIssueId(_issueInfo.IssueId);

            List<string> fullPaths = new();

            foreach (int fileId in fileIds)
            {
                string fullPath = GetFullPath(fileId);
                fullPaths.Add(fullPath);
            }

            FileListBox.ItemsSource = fullPaths;
        }

        private string GetFullPath(int fileId)
        {
            List<string> pathParts = new();

            while (fileId != -1)
            {
                var fileInfo = _controller.FileRepository.specific_file_info(fileId);
                if (fileInfo == null)
                    break;

                pathParts.Insert(0, fileInfo.FileName);
                fileId = fileInfo.ParentFolderId;
            }

            return "/" + string.Join("/", pathParts);
        }

        private void LoadComments()
        {
            var commentList = _controller.FileIssueCommentRepository.GetCommentsByIssueId(_issueInfo.IssueId);
            CommentListBox.ItemsSource = commentList;
        }

        private void AddCommentButton_Click(object sender, RoutedEventArgs e)
        {
            var input = Microsoft.VisualBasic.Interaction.InputBox("댓글을 입력하세요:", "코멘트 추가", "");
            if (!string.IsNullOrWhiteSpace(input))
            {
                var newComment = new FileIssueComment
                {
                    IssueId = _issueInfo.IssueId,
                    CommenterId = _controller.user_id,
                    Content = input,
                    CreatedAt = DateTime.Now
                };

                _controller.FileIssueCommentRepository.AddComment(newComment);
                LoadComments();
            }
        }

        private void EditIssueButton_Click(object sender, RoutedEventArgs e)
        {
            RightDetailArea.Content = new IssueInfoEditView(_controller, _issueInfo, this);
        }

        private void DeleteIssueButton_Click(object sender, RoutedEventArgs e)
        {
            var confirm = System.Windows.MessageBox.Show("정말 이 이슈를 삭제하시겠습니까?", "이슈 삭제", MessageBoxButton.YesNo);
            if (confirm == MessageBoxResult.Yes)
            {
                _controller.FileIssueMappingRepository.DeleteMappingsByIssueId(_issueInfo.IssueId);
                _controller.FileIssueRepository.DeleteIssue(_issueInfo.IssueId);
                System.Windows.MessageBox.Show("이슈 삭제 완료");

                // 부모 창 닫기
                _parentWindow.Close();
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            _parentWindow.Close();
        }

        public void SwitchRight(System.Windows.Controls.UserControl view)
        {
            RightDetailArea.Content = view;
        }
    }
}
