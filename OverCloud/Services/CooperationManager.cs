using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using OverCloud.Services.FileManager.DriveManager;
using overcloud.Views;

namespace OverCloud.Services
{
    public class CooperationManager
    {

        private bool Join_cooperation_Cloud_Storage_UI_to_pro(string user_id, string password)
        {
            if (LoginWindow._CooperationRepository != null)
            {
                //return LoginWindow._CooperationRepository.Join_cooperation_Cloud_Storage_pro_to_DB( user_id,  password);
                return true;
            }
            return false;
        }

        private bool Add_cooperation_Cloud_Storage_UI_to_pro(string user_id_insert, string password, string user_id_mine)
        {
            if (LoginWindow._CooperationRepository != null)
            {
                return LoginWindow._CooperationRepository.Add_cooperation_Cloud_Storage_pro_to_DB(user_id_insert, password, user_id_mine);
            }
            return false;
        }

        private bool Delete_cooperation_Cloud_Storage_UI_to_pro(string user_id_insert, string user_id_mine)
        {
            if (LoginWindow._CooperationRepository != null)
            {
                return LoginWindow._CooperationRepository.Delete_cooperation_Cloud_Storage_pro_to_DB( user_id_insert,  user_id_mine);
            }
            return false;
        }
    }

}