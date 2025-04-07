using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;

namespace OverCloud.Services
{
    // CloudStorageMerger: 통합 용량 계산
    //실제 DB에 영향을 주진 않지만 , UI에 통합 용량 표시 보여주기용 클래스임

    //호출 방법 예시
    //CloudStorageMerger cl = new CloudStorageMerger(repo);
    //ulong total = cl.TotalCapacity;
    public class CloudStorageMerger
    {
        private readonly IAccountRepository repo;
        private List<CloudStorageInfo> accounts;

        public CloudStorageMerger(IAccountRepository repo)
        {
            this.repo = repo;
            LoadAccounts();
        }

        public void LoadAccounts()
        {
            accounts = repo.GetAllAccounts();
        }

        public ulong TotalCapacity => (ulong)accounts.Sum(a => a.TotalCapacity);
        public ulong UsedCapacity => (ulong)accounts.Sum(a => a.UsedCapacity);
        public ulong Remaining => TotalCapacity - UsedCapacity;
        public double UsagePercent => TotalCapacity == 0 ? 0 : (double)UsedCapacity / TotalCapacity * 100;
    }

}
