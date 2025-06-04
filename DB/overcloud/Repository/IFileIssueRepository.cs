public interface IFileIssueRepository
{
    int AddIssue(FileIssueInfo issue);
    List<FileIssueInfo> GetAllIssues(string ID);
    List<FileIssueInfo> GetIssuesByFileId(int fileId);
    bool UpdateIssueStatus(int issueId, string status);
    bool AssignIssue(int issueId, string assignedTo);
    bool DeleteIssue(int issueId);

    int AddComment(FileIssueComment comment);
    List<FileIssueComment> GetCommentsByIssueId(int issueId);
}
