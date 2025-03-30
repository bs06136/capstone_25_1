namespace DB.overcloud.Models
{
    public class CloudAccountInfo
    {
        public int UserNum { get; set; }
        public string ID { get; set; }
        public string Password { get; set; }
        public string CloudType { get; set; }


        public long TotalSize { get; set; }
        public long UsedSize { get; set; }
    }
}


