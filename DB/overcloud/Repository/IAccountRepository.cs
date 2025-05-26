using DB.overcloud.Models;

namespace DB.overcloud.Repository
{
    public interface IAccountRepository
    {
        List<CloudStorageInfo> GetAllAccounts(string ID);
        bool DeleteAccountById(string ID);
        bool UpdateAccountUsage(string ID, ulong totalSize, ulong usedSize);
        bool assign_overcloud(string ID, string password);
        string login_overcloud(string ID, string password);
    }
}
