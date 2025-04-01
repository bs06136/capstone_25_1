namespace DB.overcloud.Models
{
    public class CloudAccountInfo
    {
        public int UserNum { get; set; }              // 사용자 고유 ID (PK)
        public string Username { get; set; }          // 사용자 이름
        public string ID { get; set; }                // 이메일 또는 ID
        public string Password { get; set; }          // 비밀번호

        public ulong TotalSize { get; set; }          // 전체 클라우드 계정 총 용량
        public ulong UsedSize { get; set; }           // 전체 사용량
    }
}
