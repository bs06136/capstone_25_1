namespace DB.overcloud.Models
{
    public class CloudAccountInfo
    {
        public string ID { get; set; }                        // 오버클라우드 사용자 ID
        public string Password { get; set; }                  // 오버클라우드 사용자 비밀번호
        public ulong? TotalSize { get; set; }                 // 전체 클라우드 용량
        public ulong? UsedSize { get; set; }                  // 전체 클라우드 사용량
        public bool IsShared { get; set; }                    // 협업 클라우드 여부
    }
}
