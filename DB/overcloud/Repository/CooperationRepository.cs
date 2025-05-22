using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

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
                string insertAccountQuery = @"
                    INSERT INTO Account (
                        ID, password, is_shared
                    ) VALUES (
                        @insert_id, @pw, 1
                    );";

                using var insertAccountCmd = new MySqlCommand(insertAccountQuery, conn, transaction);
                insertAccountCmd.Parameters.AddWithValue("@insert_id", user_id_insert);
                insertAccountCmd.Parameters.AddWithValue("@pw", password);
                insertAccountCmd.ExecuteNonQuery();

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

            // 1. user_id_insert가 협업 클라우드 계정인지 확인
            string checkSharedQuery = @"
                SELECT ID FROM Account 
                WHERE ID = @id AND is_shared = 1
                LIMIT 1";

            using var checkCmd = new MySqlCommand(checkSharedQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", user_id_insert);

            object checkResult = checkCmd.ExecuteScalar();
            if (checkResult == null) return false;

            // 2. Cooperation 테이블에서 해당 협업 클라우드 연결 제거
            string deleteQuery = @"
                DELETE FROM Cooperation 
                WHERE cloud_storage_num = @cloud_id";

            using var delCmd = new MySqlCommand(deleteQuery, conn);
            delCmd.Parameters.AddWithValue("@cloud_id", user_id_insert);

            return delCmd.ExecuteNonQuery() > 0;
        }

        public List<int> connected_cooperation_account_nums(string user_id)
        {
            var result = new List<int>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT cloud_storage_num 
                FROM Cooperation 
                WHERE ID = @id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", user_id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(Convert.ToInt32(reader["cloud_storage_num"]));
            }

            return result;
        }
    }
}
