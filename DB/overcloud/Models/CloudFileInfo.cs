namespace DB.overcloud.Models
{
    public class CloudFileInfo
    {
        public int FileId { get; set; }                     // 파일 또는 폴더의 고유 ID (PK)
        public string FileName { get; set; }                // 이름 (파일명 or 폴더명)
        public string FilePath { get; set; }                // 전체 경로 문자열
        public ulong FileSize { get; set; }                 // 파일 크기 (바이트, 폴더는 0)
        public DateTime UploadedAt { get; set; }            // 업로드된 날짜 및 시간
        
        public string CloudType { get; set; }               // 클라우드 종류 (GoogleDrive 등)
        public int CloudStorageNum { get; set; }            // 연결된 클라우드 계정 ID (FK)
        public int? ParentFolderId { get; set; }            // 부모 폴더의 ID (자기참조)

        public bool IsFolder { get; set; }                  // 폴더 여부

        public int count { get; set; }
        //public string GoogleFileId { get; set; }            // Google Drive 연동용 식별자
    }
}
