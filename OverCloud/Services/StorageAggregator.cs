using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using overcloud;

namespace OverCloud.Services
{
    //StorageUpdater: API에서 받아온 실제 클라우드의 사용량 데이터를 DB에 갱신하는 역할
    public class StorageAggregator
    {
        private readonly IStorageRepository storageRepository;
        private readonly IAccountRepository accountRepository;


        public StorageAggregator()
        {
            accountRepository = new AccountRepository(DbConfig.ConnectionString);
            storageRepository = new StorageRepository(DbConfig.ConnectionString);
        }

        /// <summary>
        /// 하나의 사용자 계정에 속한 모든 클라우드 스토리지들의 용량을 합산하고 DB(Account 테이블)에 반영
        /// </summary>
        public bool UpdateAggregatedStorageForUser(string userId) //여기서 넘기는 userId는 overcloud계정의 id임요
        {
            // 1. 해당 계정이 가진 모든 클라우드 가져오기
            var cloudList = storageRepository.GetCloudsForUser(userId);
            if (cloudList == null || cloudList.Count == 0)
                return false;

            int userNum = cloudList.First().UserNum;

            // 2. 합산
            ulong totalSize = (ulong)cloudList.Sum(c => c.TotalCapacity);
            ulong usedSize = (ulong)cloudList.Sum(c => c.UsedCapacity);

            // 3. DB 업데이트
            return accountRepository.UpdateAccountUsage(userNum, totalSize, usedSize);
        }
    }
}