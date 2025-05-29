using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public class StorageRepository : IStorageRepository
    {
        private readonly string connectionString;

        public StorageRepository(string connStr)
        {
            connectionString = connStr;
        }
        
        // cloud_storage_num 을 생성하거나 기존 것을 반환만 함 (INSERT 는 외부에서 수행)
        public int GetOrCreateCloudStorageNum(string cloudType, string accountId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 동일한 클라우드가 있는지 확인
            string checkQuery = @"
                SELECT cloud_storage_num 
                FROM CloudStorageInfo 
                WHERE cloud_type = @type AND account_id = @account_id 
                LIMIT 1";

            using var checkCmd = new MySqlCommand(checkQuery, conn);
            checkCmd.Parameters.AddWithValue("@type", cloudType);
            checkCmd.Parameters.AddWithValue("@account_id", accountId);

            var existingResult = checkCmd.ExecuteScalar();

            if (existingResult != null)
            {
                return Convert.ToInt32(existingResult);
            }
            
            else
            {
                string maxQuery = "SELECT COALESCE(MAX(cloud_storage_num), 0) FROM CloudStorageInfo";
                using var maxCmd = new MySqlCommand(maxQuery, conn);
                return Convert.ToInt32(maxCmd.ExecuteScalar()) + 1;
            }
        }

        public CloudStorageInfo GetCloud(int cloudStorageNum, string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudStorageInfo WHERE cloud_storage_num = @num AND ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@num", cloudStorageNum);
            cmd.Parameters.AddWithValue("@id", userId);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new CloudStorageInfo
                {
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ID = reader["ID"].ToString(),
                    CloudType = reader["cloud_type"].ToString(),
                    AccountId = reader["account_id"].ToString(),
                    AccountPassword = reader["account_password"].ToString(),
                    TotalCapacity = reader["total_capacity"] != DBNull.Value ? Convert.ToUInt64(reader["total_capacity"]) : 0,
                    UsedCapacity = reader["used_capacity"] != DBNull.Value ? Convert.ToUInt64(reader["used_capacity"]) : 0,
                    RefreshToken = reader["refresh_token"]?.ToString(),
                    ClientId = reader["client_id"]?.ToString(),
                    ClientSecret = reader["client_secret"]?.ToString()
                };
            }

            return null;
        }

        public bool AddCloudStorage(CloudStorageInfo info, string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            int cloudStorageNum = GetOrCreateCloudStorageNum(info.CloudType, info.AccountId);
            info.CloudStorageNum = cloudStorageNum;

            string insertCloudStorageQuery = @"INSERT INTO CloudStorageInfo 
                (cloud_storage_num, ID, cloud_type, account_id, account_password, total_capacity, used_capacity, refresh_token, client_id, client_secret)
                VALUES 
                (@num, @id, @type, @accountId, @accountPw, @total, @used, @refresh, @clientId, @clientSecret)";

            using var cmd = new MySqlCommand(insertCloudStorageQuery, conn);
            cmd.Parameters.AddWithValue("@num", info.CloudStorageNum);
            cmd.Parameters.AddWithValue("@id", info.ID);
            cmd.Parameters.AddWithValue("@type", info.CloudType);
            cmd.Parameters.AddWithValue("@accountId", info.AccountId);
            cmd.Parameters.AddWithValue("@accountPw", info.AccountPassword);
            cmd.Parameters.AddWithValue("@total", info.TotalCapacity);
            cmd.Parameters.AddWithValue("@used", info.UsedCapacity);
            cmd.Parameters.AddWithValue("@refresh", info.RefreshToken ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientId", info.ClientId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientSecret", info.ClientSecret ?? (object)DBNull.Value);

            if (cmd.ExecuteNonQuery() <= 0)
                return false;

            // 협업 클라우드 계정 추가 여부 판단
            if (info.ID != userId)
            {
                // CoopUserInfo에서 coop_num 조회
                string selectCoopNumQuery = @"SELECT coop_num FROM CoopUserInfo 
                                            WHERE coop_id = @coopId AND user_id = @userId";

                using var selectCmd = new MySqlCommand(selectCoopNumQuery, conn);
                selectCmd.Parameters.AddWithValue("@coopId", info.ID);
                selectCmd.Parameters.AddWithValue("@userId", userId);

                object result = selectCmd.ExecuteScalar();

                if (result == null)
                    return false;

                int coopNum = Convert.ToInt32(result);

                // CoopStorageInfo에 삽입
                string insertCoopStorageQuery = @"INSERT INTO CoopStorageInfo 
                    (coop_num, cloud_storage_num, ID) 
                    VALUES (@coopNum, @cloudNum, @id)";

                using var insertCoopCmd = new MySqlCommand(insertCoopStorageQuery, conn);
                insertCoopCmd.Parameters.AddWithValue("@coopNum", coopNum);
                insertCoopCmd.Parameters.AddWithValue("@cloudNum", info.CloudStorageNum);
                insertCoopCmd.Parameters.AddWithValue("@id", info.ID);

                if (insertCoopCmd.ExecuteNonQuery() <= 0)
                    return false;
            }

            return true;
        }

        public bool DeleteCloudStorage(int cloudStorageNum, string userId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                // Step 1: CoopStorageInfo에서 존재 여부 확인
                string checkCoopQuery = @"SELECT COUNT(*) FROM CoopStorageInfo 
                                        WHERE cloud_storage_num = @num AND ID = @id";

                using var checkCmd = new MySqlCommand(checkCoopQuery, conn, transaction);
                checkCmd.Parameters.AddWithValue("@num", cloudStorageNum);
                checkCmd.Parameters.AddWithValue("@id", userId);

                long count = (long)checkCmd.ExecuteScalar();

                if (count > 0)
                {
                    // Step 2: CoopStorageInfo에서 먼저 삭제
                    string deleteCoopQuery = @"DELETE FROM CoopStorageInfo 
                                            WHERE cloud_storage_num = @num AND ID = @id";

                    using var deleteCoopCmd = new MySqlCommand(deleteCoopQuery, conn, transaction);
                    deleteCoopCmd.Parameters.AddWithValue("@num", cloudStorageNum);
                    deleteCoopCmd.Parameters.AddWithValue("@id", userId);
                    deleteCoopCmd.ExecuteNonQuery();
                }

                // Step 3: CloudStorageInfo 삭제 (공통)
                string deleteCloudQuery = @"DELETE FROM CloudStorageInfo 
                                            WHERE cloud_storage_num = @num AND ID = @id";

                using var deleteCloudCmd = new MySqlCommand(deleteCloudQuery, conn, transaction);
                deleteCloudCmd.Parameters.AddWithValue("@num", cloudStorageNum);
                deleteCloudCmd.Parameters.AddWithValue("@id", userId);

                int affected = deleteCloudCmd.ExecuteNonQuery();

                transaction.Commit();

                return affected > 0;
            }
            catch
            {
                transaction.Rollback();
                return false;
            }
        }

        public bool account_save(CloudStorageInfo one_cloud)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE CloudStorageInfo SET 
                                total_capacity = @total,
                                used_capacity = @used
                            WHERE cloud_storage_num = @cloudNum AND ID = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@total", one_cloud.TotalCapacity);
            cmd.Parameters.AddWithValue("@used", one_cloud.UsedCapacity);
            cmd.Parameters.AddWithValue("@cloudNum", one_cloud.CloudStorageNum);
            cmd.Parameters.AddWithValue("@id", one_cloud.ID);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateRefreshToken(int cloudStorageNum, string userId, string refreshToken)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE CloudStorageInfo 
                            SET refresh_token = @token 
                            WHERE cloud_storage_num = @id AND ID = @userId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@token", refreshToken);
            cmd.Parameters.AddWithValue("@id", cloudStorageNum);
            cmd.Parameters.AddWithValue("@userId", userId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}