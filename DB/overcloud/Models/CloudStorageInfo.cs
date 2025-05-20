namespace DB.overcloud.Models
{
    public class CloudStorageInfo
    {
        public int CloudStorageNum { get; set; }              // PK
        public string ID { get; set; }                        // FK: Account.ID
        public string CloudType { get; set; }                 // 클라우드 종류
        public string AccountId { get; set; }                 // 클라우드 계정 ID
        public string AccountPw { get; set; }                 // 클라우드 계정 PW
        public ulong? TotalCapacity { get; set; }             // 단일 클라우드 총 용량
        public ulong? UsedCapacity { get; set; }              // 단일 클라우드 사용 용량
        public string? RefreshToken { get; set; }             // OAuth 토큰
        public string? ClientId { get; set; }                 // 클라이언트 ID
        public string? ClientSecret { get; set; }             // 클라이언트 Secret
    }
}
