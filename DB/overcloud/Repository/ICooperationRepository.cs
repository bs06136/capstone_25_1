using DB.overcloud.Models;

namespace DB.overcloud.Repository
{
    public interface ICooperationRepository
    {
        bool Add_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string password, string user_id_mine);
    }
}
