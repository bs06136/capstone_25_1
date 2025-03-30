using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Data
{
    internal interface IAccountRepositiory //
    {
        bool InsertAccount(CloudAccountInfo account);
        List<CloudAccountInfo> GetAllAccounts();
        bool DeleteAccountByUserNum(int userNum);
    }
}
