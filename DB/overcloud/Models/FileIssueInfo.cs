public class FileIssueInfo
{
    public int IssueId { get; set; }             // PK
    public string ID { get; set; }               // 협업 클라우드 ID
    public string Title { get; set; }            // 이슈 제목
    public string Description { get; set; }      // 상세 내용
    public string CreatedBy { get; set; }        // 이슈 작성자
    public string AssignedTo { get; set; }       // 담당자 (nullable)
    public string Status { get; set; }           // 이슈 상태 (OPEN, IN_PROGRESS, RESOLVED, CLOSED)
    public DateTime CreatedAt { get; set; }      // 생성일시
    public DateTime? DueDate { get; set; }       // 마감 기한 (nullable)
}
