using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;

namespace DB.overcloud.Repository
{
    public class CoopUserRepository : ICoopUserRepository
    {
        private readonly string connectionString;

        public CoopUserRepository(string connectionString)
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
                // 1. Account 테이블에 협업 클라우드 계정 추가
                string insertAccountQuery = @"
                    INSERT INTO Account (ID, password)
                    VALUES (@insert_id, @pw);";

                using var accountCmd = new MySqlCommand(insertAccountQuery, conn, transaction);
                accountCmd.Parameters.AddWithValue("@insert_id", user_id_insert);
                accountCmd.Parameters.AddWithValue("@pw", password);
                accountCmd.ExecuteNonQuery();

                // 2. CoopUserInfo 테이블에 생성자(user_id_mine)를 참여자로 등록
                string insertCoopUserQuery = @"
                    INSERT INTO CoopUserInfo (coop_id, user_id)
                    VALUES (@coop_id, @user_id);";

                using var coopUserCmd = new MySqlCommand(insertCoopUserQuery, conn, transaction);
                coopUserCmd.Parameters.AddWithValue("@coop_id", user_id_insert);
                coopUserCmd.Parameters.AddWithValue("@user_id", user_id_mine);
                coopUserCmd.ExecuteNonQuery();

                // 3. CloudStorageInfo에 시스템 전용 더미 계정 삽입
                string insertDummyStorageQuery = @"INSERT INTO CloudStorageInfo
                    (cloud_storage_num, ID, cloud_type, account_id, account_password, total_capacity, used_capacity)
                    VALUES
                    (-1, @id, 'SYSTEM', '', '', 0, 0)";

                using var dummyCmd = new MySqlCommand(insertDummyStorageQuery, conn, transaction);
                dummyCmd.Parameters.AddWithValue("@id", user_id_insert);
                dummyCmd.ExecuteNonQuery();

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

            // 1. 해당 협업 클라우드에 해당 사용자가 참여 중인지 확인
            string checkQuery = @"SELECT coop_num FROM CoopUserInfo 
                                WHERE coop_id = @coop_id AND user_id = @user_id";

            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@coop_id", user_id_insert);
            checkCmd.Parameters.AddWithValue("@user_id", user_id_mine);

            object checkResult = checkCmd.ExecuteScalar();
            if (checkResult == null) return false;

            // 2. CoopUserInfo에서 연결 해제
            string deleteQuery = @"DELETE FROM CoopUserInfo 
                                WHERE coop_id = @coop_id AND user_id = @user_id";

            using var delCmd = new MySqlCommand(deleteQuery, conn);
            delCmd.Parameters.AddWithValue("@coop_id", user_id_insert);
            delCmd.Parameters.AddWithValue("@user_id", user_id_mine);

            return delCmd.ExecuteNonQuery() > 0;
        }

        public bool Join_cooperation_Cloud_Storage_pro_to_DB(string user_id_insert, string password, string user_id_mine)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 1. 협업 클라우드 ID와 비밀번호 확인 (is_shared = 1)
            string checkQuery = @"
                SELECT ID FROM Account
                WHERE ID = @id AND password = @pw";

            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@id", user_id_insert);
            checkCmd.Parameters.AddWithValue("@pw", password);

            object result = checkCmd.ExecuteScalar();
            if (result == null) return false;

            // 2. CoopUserInfo에 참여자 등록
            string insertQuery = @"
                INSERT INTO CoopUserInfo (coop_id, user_id)
                VALUES (@coop_id, @user_id);";

            using var insertCmd = new MySqlCommand(insertQuery, conn);
            insertCmd.Parameters.AddWithValue("@coop_id", user_id_insert);
            insertCmd.Parameters.AddWithValue("@user_id", user_id_mine);

            return insertCmd.ExecuteNonQuery() > 0;
        }

        public List<string> connected_cooperation_account_nums(string user_id)
        {
            var result = new List<string>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT coop_id 
                FROM CoopUserInfo 
                WHERE user_id = @user_id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user_id", user_id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["coop_id"].ToString());
            }

            return result;
        }

        public List<string> GetUsersByCoopId(string coop_id)
        {
            var result = new List<string>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
                SELECT user_id
                FROM CoopUserInfo
                WHERE coop_id = @coop_id;";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@coop_id", coop_id);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(reader["user_id"].ToString());
            }

            return result;
        }

    }
}