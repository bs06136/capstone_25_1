public class FileIssueInfo
{
    public int IssueId { get; set; }             // issue_id
    public int FileId { get; set; }              // file_id
    public string Title { get; set; }            // 이슈 제목
    public string Description { get; set; }      // 상세 설명
    public string CreatedBy { get; set; }        // 작성자 ID
    public string AssignedTo { get; set; }       // 담당자 ID (nullable)
    public string Status { get; set; }           // OPEN, IN_PROGRESS, RESOLVED, CLOSED
    public DateTime CreatedAt { get; set; }      // 생성일시
    public DateTime? DueDate { get; set; }       // 마감일 (nullable)
}
