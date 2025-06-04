using DB.overcloud.Models;
using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;

namespace DB.overcloud.Repository
{
    public class FileIssueCommentRepository : IFileIssueCommentRepository
    {
        private readonly string connectionString;

        public FileIssueCommentRepository(string connectionString)
        {
            this.connectionString = connectionString;
        }

        public bool AddComment(FileIssueComment comment)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
            INSERT INTO FileIssueComment (issue_id, writer_id, content, created_at)
            VALUES (@issueId, @writerId, @content, @createdAt)";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", comment.IssueId);
            cmd.Parameters.AddWithValue("@writerId", comment.WriterId);
            cmd.Parameters.AddWithValue("@content", comment.Content);
            cmd.Parameters.AddWithValue("@createdAt", comment.CreatedAt);

            return cmd.ExecuteNonQuery() > 0;
        }

        public List<FileIssueComment> GetCommentsByIssueId(int issueId)
        {
            var comments = new List<FileIssueComment>();
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = @"
            SELECT * FROM FileIssueComment
            WHERE issue_id = @issueId
            ORDER BY created_at ASC";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@issueId", issueId);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var comment = new FileIssueComment
                {
                    CommentId = Convert.ToInt32(reader["comment_id"]),
                    IssueId = Convert.ToInt32(reader["issue_id"]),
                    WriterId = reader["writer_id"].ToString(),
                    Content = reader["content"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                };

                comments.Add(comment);
            }

            return comments;
        }

        public bool DeleteComment(int commentId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "DELETE FROM FileIssueComment WHERE comment_id = @commentId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@commentId", commentId);

            return cmd.ExecuteNonQuery() > 0;
        }

        public FileIssueComment? GetCommentById(int commentId)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "SELECT * FROM FileIssueComment WHERE comment_id = @commentId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@commentId", commentId);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new FileIssueComment
                {
                    CommentId = Convert.ToInt32(reader["comment_id"]),
                    IssueId = Convert.ToInt32(reader["issue_id"]),
                    WriterId = reader["writer_id"].ToString(),
                    Content = reader["content"].ToString(),
                    CreatedAt = Convert.ToDateTime(reader["created_at"])
                };
            }

            return null;
        }

        public bool UpdateComment(int commentId, string newContent)
        {
            using var conn = new MySqlConnection(connectionString);
            conn.Open();

            string query = "UPDATE FileIssueComment SET content = @content WHERE comment_id = @commentId";

            using var cmd = new MySqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@content", newContent);
            cmd.Parameters.AddWithValue("@commentId", commentId);

            return cmd.ExecuteNonQuery() > 0;
        }
    }
}