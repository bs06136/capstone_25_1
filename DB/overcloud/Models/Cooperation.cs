namespace DB.overcloud.Models
{
    public class Cooperation
    {
        public string ID { get; set; }                 // 사용자 ID: Account.ID
        public int CloudStorageNum { get; set; }       // 클라우드 번호: CloudStorageInfo.cloud_storage_num
    }
}