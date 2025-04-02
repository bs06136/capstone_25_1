using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public interface IAccountRepository
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudStorageInfo> GetAllCloudAccounts();
        bool DeleteAccountByUserNum(int userNum);
        bool UpdateAccountUsage(int userNum, ulong totalSize, ulong usedSize)
    }
}
