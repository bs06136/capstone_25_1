public class FileIssueComment
{
    public int CommentId { get; set; }           // comment_id (PK)
    public int IssueId { get; set; }             // 연결된 이슈 ID (FK -> FileIssueInfo)
    public string CommenterId { get; set; }      // 작성자 ID
    public string Content { get; set; }          // 댓글 본문
    public DateTime CreatedAt { get; set; }      // 작성 시간
}
