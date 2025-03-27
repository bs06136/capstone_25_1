namespace overcloud.Models
{
    public class CloudStorageInfo
    {
        public string ServiceName { get; set; }    // 클라우드 서비스명 (예: Google Drive)
        public long TotalSize { get; set; }        // 최대 용량 (바이트)
        public long UsedSize { get; set; }         // 사용 중인 용량 (바이트)

        // 사용 용량 백분율 계산 (편의를 위한 속성)
        public double UsagePercent => TotalSize == 0 ? 0 : (double)UsedSize / TotalSize * 100;
    }
}
