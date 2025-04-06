namespace DB.overcloud.Models
{
    public class CloudStorageInfo
    {
        public int CloudStorageNum { get; set; }         // 클라우드 계정 고유 번호 (PK)
        public int UserNum { get; set; }                 // 소유 사용자 (Account.user_num) (FK -> CloudAccountInfo)

        public string CloudType { get; set; }            // 클라우드 종류 (예: GoogleDrive)
        public string AccountId { get; set; }            // 클라우드 로그인 ID
        public string AccountPassword { get; set; }      // 클라우드 로그인 비밀번호 (필요시)

        public int TotalCapacity { get; set; }             // 계정 한 개의 전체 용량
        public int UsedCapacity { get; set; }              // 계정 한 개의 사용 중인 용량

        public string AccessToken { get; set; }         // Google Drive API 접근 토큰
    }
}
