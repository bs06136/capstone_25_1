using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Service
{
    public class FileRepository : IFileRepository
    {
        private readonly string connectionString;

        public FileRepository(string connStr)
        {
            connectionString = connStr;
        }

        public List<CloudFileInfo> GetAllFileInfo()
        {
            var list = new List<CloudFileInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudFileInfo";
            using var cmd = new MySqlCommand(query, conn);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new CloudFileInfo
                {
                    FileId = Convert.ToInt32(reader["file_id"]),
                    FileName = reader["file_name"].ToString(),
                    FilePath = reader["file_path"]?.ToString(),
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudType = reader["cloud_type"].ToString(),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = reader["parent_folder_id"] == DBNull.Value ? null : Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"])
                });
            }

            return list;
        }

        public bool AddFile(CloudFileInfo file)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"INSERT INTO CloudFileInfo 
                (file_name, file_path, file_size, uploaded_at, cloud_type, cloud_storage_num, parent_folder_id, is_folder)
                VALUES 
                (@name, @path, @size, @time, @type, @storage, @parent, @folder)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", file.FileName);
            cmd.Parameters.AddWithValue("@path", file.FilePath ?? "");
            cmd.Parameters.AddWithValue("@size", file.FileSize);
            cmd.Parameters.AddWithValue("@time", file.UploadedAt);
            cmd.Parameters.AddWithValue("@type", file.CloudType);
            cmd.Parameters.AddWithValue("@storage", file.CloudStorageNum);
            cmd.Parameters.AddWithValue("@parent", file.ParentFolderId.HasValue ? file.ParentFolderId.Value : (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@folder", file.IsFolder);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool DeleteFile(int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM CloudFileInfo WHERE file_id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", fileId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public CloudFileInfo GetFileById(int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudFileInfo WHERE file_id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", fileId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new CloudFileInfo
                {
                    FileId = Convert.ToInt32(reader["file_id"]),
                    FileName = reader["file_name"].ToString(),
                    FilePath = reader["file_path"]?.ToString(),
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudType = reader["cloud_type"]?.ToString(),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = reader["parent_folder_id"] == DBNull.Value ? null : Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"])
                    //GoogleFileId = reader["google_file_id"]?.ToString()
                };
            }

            return null;
        }
    }
}
