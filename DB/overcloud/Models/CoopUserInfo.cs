namespace DB.overcloud.Models
{
    public class CoopUserInfo
    {
        public int CoopNum { get; set; }             // PK, AUTO_INCREMENT
        public string CoopId { get; set; }           // 협업 클라우드 ID : FK -> Account.ID)
        public string UserId { get; set; }           // 사용자 ID : FK -> Account.ID
    }
}