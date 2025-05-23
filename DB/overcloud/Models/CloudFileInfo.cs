namespace DB.overcloud.Models
{
    public class CloudFileInfo
    {
        public int FileId { get; set; }                     // 파일 또는 폴더의 고유 ID (PK)
        public string FileName { get; set; }                // 이름 (파일명 or 폴더명)
        public ulong FileSize { get; set; }                 // 파일 크기 (KB 단위), 폴더는 0)
        public DateTime UploadedAt { get; set; }            // 업로드된 날짜 및 시간
        public int CloudStorageNum { get; set; }            // 연결된 클라우드 계정 ID (FK -> CloudStorageInfo)
        public string ID { get; set; }                      // 파일의 소유자 ID (FK -> Account)
        public int ParentFolderId { get; set; }             // 부모 폴더의 ID (자기참조)
        public bool IsFolder { get; set; }                  // 폴더 여부
        public string? CloudFileId { get; set; }            // 실제 구글 ,원드라이브 파일 ID
        public int? RootFileId { get; set; }                // 조각일 경우, 논리 파일 ID (자기참조)
        public int? ChunkIndex { get; set; }                // 조각 순서
        public ulong? ChunkSize { get; set; }               // 조각 실제 크기
        public bool IsDistributed { get; set; }             // 분산 저장 여부
    }
}
