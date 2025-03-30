using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Services
{
    public class CloudAccountMerger //병합 담당
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

      //  public List<CloudAccountInfo> GetAccounts() => accounts;
    }

}
