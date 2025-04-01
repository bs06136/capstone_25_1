namespace overcloud.Models
{
    public class CloudStorageInfo
    {
        public int Id { get; set; }
        public int AccountUserNum { get; set; }
        public long TotalCapacity { get; set; }
        public long UsedCapacity { get; set; }
    }
}
