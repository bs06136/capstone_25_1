using MySql.Data.MySqlClient;
using System;

namespace DB.overcloud.Repository
{
    public class CooperationRepository
    {
        private readonly string connectionString;

        public CooperationRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool Add_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string password, string user_id_mine)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // 1. CloudStorageInfo에 협업 클라우드 정보 추가
                string insertCloudQuery = @"
                    INSERT INTO CloudStorageInfo (
                        ID, cloud_type, account_id, account_password, 
                        total_capacity, used_capacity, is_shared
                    ) VALUES (
                        @insert_id, 'COOP', @insert_id, @pw, 0, 0, 1
                    );
                    SELECT LAST_INSERT_ID();";

                using var insertCloudCmd = new MySqlCommand(insertCloudQuery, conn, transaction);
                insertCloudCmd.Parameters.AddWithValue("@insert_id", user_id_insert);
                insertCloudCmd.Parameters.AddWithValue("@pw", password);

                int insertedCloudNum = Convert.ToInt32(insertCloudCmd.ExecuteScalar());

                // 2. Cooperation 테이블에 연결 정보 추가
                string insertCoopQuery = @"
                    INSERT INTO Cooperation (ID, cloud_storage_num)
                    VALUES (@mine, @cloudNum);";

                using var insertCoopCmd = new MySqlCommand(insertCoopQuery, conn, transaction);
                insertCoopCmd.Parameters.AddWithValue("@mine", user_id_mine);
                insertCoopCmd.Parameters.AddWithValue("@cloudNum", insertedCloudNum);

                insertCoopCmd.ExecuteNonQuery();

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool Delete_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string user_id_mine)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 1. cloud_storage_num 조회
            string getStorageQuery = "SELECT cloud_storage_num FROM CloudStorageInfo WHERE ID = @id AND is_shared = 1 LIMIT 1";
            using var getCmd = new MySqlCommand(getStorageQuery, conn);
            getCmd.Parameters.AddWithValue("@id", user_id_insert);

            object result = getCmd.ExecuteScalar();
            if (result == null) return false;

            int cloudStorageNum = Convert.ToInt32(result);

            // 2. Cooperation 테이블에서 연결 제거
            string deleteQuery = "DELETE FROM Cooperation WHERE ID = @mine AND cloud_storage_num = @cloudNum";
            using var delCmd = new MySqlCommand(deleteQuery, conn);
            delCmd.Parameters.AddWithValue("@mine", user_id_mine);
            delCmd.Parameters.AddWithValue("@cloudNum", cloudStorageNum);

            return delCmd.ExecuteNonQuery() > 0;
        }
        
    }
}
