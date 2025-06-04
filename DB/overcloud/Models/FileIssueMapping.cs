public class FileIssueMapping
{
    public int IssueId { get; set; }             // FK -> FileIssueInfo
    public int FileId { get; set; }              // FK -> CloudFileInfo
}