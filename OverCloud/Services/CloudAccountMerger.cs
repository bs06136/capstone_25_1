using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using overcloud.Models;

namespace OverCloud.Services
{
    //클라우드에 계정을 추가하면 총용량, 사용중인 용량들을 더하는 로직 클래스.
    public class CloudAccountMerger 
    {
        private readonly List<CloudAccountInfo> accounts;

        public CloudAccountMerger(List<CloudAccountInfo> accounts)
        {
            this.accounts = accounts;
        }

        public long TotalSize => accounts.Sum(a => a.TotalSize);
        public long UsedSize => accounts.Sum(a => a.UsedSize);
        public long RemainingSize => TotalSize - UsedSize;

        public double UsagePercent =>
            TotalSize == 0 ? 0 : (double)UsedSize / TotalSize * 100;

        public List<CloudAccountInfo> GetAccounts() => accounts;
    }

}
