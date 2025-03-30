using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace overcloud.Models
{
    public class CloudAccountInfo
    {
        public int UserNum { get; set; } //PK
        public string ID { get; set; }
        public string Password { get; set; }
        public string CloudType { get; set; }

        // 실시간으로 가져올 용량 정보
        public long TotalSize{get; set;}
        public long UsedSize { get; set; }
    }
}
