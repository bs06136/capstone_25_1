namespace DB.overcloud.Models
{
    public class CloudStorageInfo
    {
        public int CloudStorageNum { get; set; }         // 클라우드 계정 고유 번호 (PK)
        public int UserNum { get; set; }                 // 소유 사용자 (Account.user_num)

        public string CloudType { get; set; }            // 클라우드 종류 (예: GoogleDrive)
        public string AccountId { get; set; }            // 클라우드 로그인 ID
        public string AccountPassword { get; set; }      // 클라우드 로그인 비밀번호 (필요시)

        public int TotalSize { get; set; }             // 계정의 전체 용량
        public int UsedSize { get; set; }              // 계정의 사용 중인 용량

        public string accessToken { get; set; }         // Google Drive API 접근 토큰 
    }
}
