using DB.overcloud.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public class FileRepository : IFileRepository
    {
        private readonly string connectionString;

        public FileRepository(string connStr)
        {
            connectionString = connStr;
        }

        public List<CloudFileInfo> GetAllFileInfo(int fileId)
        {
            var list = new List<CloudFileInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudFileInfo WHERE parent_folder_id = @parent";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@parent", fileId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CloudFileInfo
                {
                    FileId = Convert.ToInt32(reader["file_id"]),
                    FileName = reader["file_name"].ToString(),
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"]),
                    Count = Convert.ToInt32(reader["count"]),
                });
            }

            return list;
        }

        public bool addfile(CloudFileInfo file_info)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"INSERT INTO CloudFileInfo 
                (file_name, file_size, uploaded_at, cloud_storage_num, parent_folder_id, is_folder, count, google_file_id)
                VALUES 
                (@name, @size, @time, @storage, @parent, @folder, @count, @google)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@name", file_info.FileName);
            cmd.Parameters.AddWithValue("@size", file_info.FileSize);
            cmd.Parameters.AddWithValue("@time", file_info.UploadedAt);
            cmd.Parameters.AddWithValue("@storage", file_info.CloudStorageNum);
            cmd.Parameters.AddWithValue("@parent", file_info.ParentFolderId);
            cmd.Parameters.AddWithValue("@folder", file_info.IsFolder);
            cmd.Parameters.AddWithValue("@count", 0);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool change_file(CloudFileInfo file_info, string newGoogleFileId)
        {
            if (file_info.Count < 2)
                return false;           // 다운로드 횟수가 기준 미달

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"UPDATE CloudFileInfo 
                            SET cloud_storage_num = @newStorage, google_file_id = @google 
                            WHERE file_id = @id";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@newStorage", 2);              // cloud_storage_num = 2 인 클라우드로 이동
            cmd.Parameters.AddWithValue("@google", newGoogleFileId);    // 새로 배정받은 google_file_id
            cmd.Parameters.AddWithValue("@id", file_info.FileId);

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
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"]),
                    Count = Convert.ToInt32(reader["count"]),
                };
            }

            return null;
        }

        public List<CloudFileInfo> all_file_list(int fileId)
        {
            var list = new List<CloudFileInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM CloudFileInfo WHERE parent_folder_id = @parent";

            using var cmd = new MySqlCommand(query, conn);

            cmd.Parameters.AddWithValue("@parent", fileId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new CloudFileInfo
                {
                    FileId = Convert.ToInt32(reader["file_id"]),
                    FileName = reader["file_name"].ToString(),
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"]),
                    Count = Convert.ToInt32(reader["count"]),
                });
            }

            return list;
        }

        public CloudFileInfo specific_file_info(int fileId)
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
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"]),
                    Count = Convert.ToInt32(reader["count"]),
                };
            }

            return null; // 찾는 파일이 없는 경우
        }

        public List<CloudFileInfo> GetAllFileInfo(string file_direc)
        {
            var result = new List<CloudFileInfo>();

            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            // 1. 폴더 이름으로 해당 폴더의 file_id 찾기
            string folderQuery = "SELECT file_id FROM CloudFileInfo WHERE file_name = @name AND is_folder = true LIMIT 1";
            using var folderCmd = new MySqlCommand(folderQuery, conn);
            folderCmd.Parameters.AddWithValue("@name", file_direc);

            object folderIdObj = folderCmd.ExecuteScalar();
            if (folderIdObj == null) return result; // 폴더가 존재하지 않음

            int parentId = Convert.ToInt32(folderIdObj);

            // 2. 해당 폴더에 포함된 모든 파일/폴더 조회
            string childQuery = "SELECT * FROM CloudFileInfo WHERE parent_folder_id = @parent";
            using var childCmd = new MySqlCommand(childQuery, conn);
            childCmd.Parameters.AddWithValue("@parent", parentId);

            using var reader = childCmd.ExecuteReader();
            while (reader.Read())
            {
                result.Add(new CloudFileInfo
                {
                    FileId = Convert.ToInt32(reader["file_id"]),
                    FileName = reader["file_name"].ToString(),
                    FileSize = Convert.ToUInt64(reader["file_size"]),
                    UploadedAt = Convert.ToDateTime(reader["uploaded_at"]),
                    CloudStorageNum = Convert.ToInt32(reader["cloud_storage_num"]),
                    ParentFolderId = Convert.ToInt32(reader["parent_folder_id"]),
                    IsFolder = Convert.ToBoolean(reader["is_folder"]),
                    Count = Convert.ToInt32(reader["count"]),
                });
            }

            return result;
        }

        public bool IncrementDownloadCount(int fileId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE CloudFileInfo SET count = count + 1 WHERE file_id = @id";
            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", fileId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
