using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Models
{
    public class CloudStorageInfo
    {
        public string ServiceName { get; set; }        // 예: Google Drive  
        public long TotalSize { get; set; }            // 최대 용량 (bytes)
        public long UsedSize { get; set; }             // 사용 중인 용량 (bytes)

        // 퍼센트 계산 속성
        public double UsagePercent => TotalSize == 0 ? 0 : (double)UsedSize / TotalSize * 100;
    }
}
