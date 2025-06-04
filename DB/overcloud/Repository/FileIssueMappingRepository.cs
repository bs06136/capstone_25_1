using DB.overcloud.Models;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DB.overcloud.Repository
{
    public class FileIssueMappingRepository : IFileIssueMappingRepository
    {
        private readonly string connectionString;

        public FileIssueMappingRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool AddMapping(int issueId, int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
            INSERT INTO FileIssueMapping (issue_id, file_id)
            VALUES (@issueId, @fileId)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", issueId);
            cmd.Parameters.AddWithValue("@fileId", fileId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<int> GetFileIdsByIssueId(int issueId)
        {
            var fileIds = new List<int>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT file_id FROM FileIssueMapping WHERE issue_id = @issueId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", issueId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                fileIds.Add(Convert.ToInt32(reader["file_id"]));
            }

            return fileIds;
        }

        public List<int> GetIssueIdsByFileId(int fileId)
        {
            var issueIds = new List<int>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT issue_id FROM FileIssueMapping WHERE file_id = @fileId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@fileId", fileId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                issueIds.Add(Convert.ToInt32(reader["issue_id"]));
            }

            return issueIds;
        }

        public bool DeleteMapping(int issueId, int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
            DELETE FROM FileIssueMapping
            WHERE issue_id = @issueId AND file_id = @fileId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", issueId);
            cmd.Parameters.AddWithValue("@fileId", fileId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteMappingsByIssueId(int issueId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM FileIssueMapping WHERE issue_id = @issueId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", issueId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteMappingsByFileId(int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM FileIssueMapping WHERE file_id = @fileId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@fileId", fileId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}