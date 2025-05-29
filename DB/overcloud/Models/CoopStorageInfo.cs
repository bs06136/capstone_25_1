namespace DB.overcloud.Models
{
    public class CoopStorageInfo
    {
        public int CoopNum { get; set; }             // 협업 클라우드 번호 : FK -> CoopUserInfo.CoopNum
        public int CloudStorageNum { get; set; }     // 클라우드 계정 번호 : FK -> CloudStorageInfo.cloud_storage_num
        public string ID { get; set; }               // 사용자 ID : FK -> CloudStorageInfo.ID
    }
}