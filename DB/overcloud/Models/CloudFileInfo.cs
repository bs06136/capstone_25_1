namespace DB.overcloud.Models
{
    public class CloudFileInfo
    {
        internal int count;

        public int FileId { get; set; }                     // 파일 또는 폴더의 고유 ID (PK)
        public string FileName { get; set; }                // 이름 (파일명 or 폴더명)
        public ulong FileSize { get; set; }                 // 파일 크기 (KB 단위), 폴더는 0)
        public DateTime UploadedAt { get; set; }            // 업로드된 날짜 및 시간
        public int CloudStorageNum { get; set; }            // 연결된 클라우드 계정 ID (FK -> CloudStorageInfo)
        public int ParentFolderId { get; set; }            // 부모 폴더의 ID (자기참조)
        public bool IsFolder { get; set; }                  // 폴더 여부
        public string? CloudFileId { get; set; }            // 실제 구글 드라이브 파일 ID

    }
}
