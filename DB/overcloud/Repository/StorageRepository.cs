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

        public CloudStorageInfo GetCloud(string accountId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"SELECT * FROM CloudStorageInfo WHERE account_id = @id LIMIT 1";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", accountId);

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

        public bool AddCloudStorage(CloudStorageInfo info)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"INSERT INTO CloudStorageInfo 
                (ID, cloud_type, account_id, account_password, total_capacity, used_capacity, refresh_token, client_id, client_secret)
                VALUES 
                (@id, @type, @accountId, @accountPw, @total, @used, @refresh, @clientId, @clientSecret)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", info.ID);
            cmd.Parameters.AddWithValue("@type", info.CloudType);
            cmd.Parameters.AddWithValue("@accountId", info.AccountId);
            cmd.Parameters.AddWithValue("@accountPw", info.AccountPassword);
            cmd.Parameters.AddWithValue("@total", info.TotalCapacity);
            cmd.Parameters.AddWithValue("@used", info.UsedCapacity);
            cmd.Parameters.AddWithValue("@refresh", info.RefreshToken ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientId", info.ClientId ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@clientSecret", info.ClientSecret ?? (object)DBNull.Value);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteCloudStorage(int cloudStorageNum)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM CloudStorageInfo WHERE cloud_storage_num = @num";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@num", cloudStorageNum);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool account_save(CloudStorageInfo one_cloud)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE CloudStorageInfo SET 
                                total_capacity = @total,
                                used_capacity = @used
                            WHERE cloud_storage_num = @cloudNum";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@total", one_cloud.TotalCapacity);
            cmd.Parameters.AddWithValue("@used", one_cloud.UsedCapacity);
            cmd.Parameters.AddWithValue("@cloudNum", one_cloud.CloudStorageNum);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool UpdateRefreshToken(int cloudStorageNum, string refreshToken)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE CloudStorageInfo 
                            SET refresh_token = @token 
                            WHERE cloud_storage_num = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@token", refreshToken);
            cmd.Parameters.AddWithValue("@id", cloudStorageNum);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
