using DB.overcloud.Models;

namespace DB.overcloud.Repository
{
    public interface ICooperationRepository
    {
        bool Add_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string password, string user_id_mine);
        bool Delete_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string user_id_mine);
        List<int> connected_cooperation_account_nums(string user_id);
    }
}
