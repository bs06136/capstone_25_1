using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OverCloud.Models;

namespace OverCloud.Services
{
    //클라우드 용량 계산, 분산저장 등
    public class StorageService
    {
        private readonly AccountService accountService = new AccountService();
        private readonly CloudApiService cloudApi = new CloudApiService();

        // 모든 클라우드 계정을 통합해서, 총 용량과 사용량을 계산
        public CloudStorageInfo GetUnifiedStorage()
        {
            var accounts = accountService.GetAllAccounts();

            long total = 0;
            long used = 0;

            foreach (var account in accounts)
            {
                var (accTotal, accUsed) = cloudApi.GetStorageInfo(account);

                total += accTotal;
                used += accUsed;

                // 개별 계정에도 반영해두면 이후 UI에서 쓸 수 있음
                account.TotalSize = accTotal;
                account.UsedSize = accUsed;
            }

            return new CloudStorageInfo
            {
                ServiceName = "OverCloud 통합 저장소",
                TotalSize = total,
                UsedSize = used
            };
        }
    }
}
