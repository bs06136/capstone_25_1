public class FileIssueRepository : IFileIssueRepository
{
    private readonly string connectionString;

    public FileIssueRepository(string connectionString)
    {
        this.connectionString = connectionString;
    }

    public int AddIssue(FileIssueInfo issue)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = @"INSERT INTO FileIssueInfo 
            (file_id, ID, title, description, created_by, assigned_to, status, created_at, due_date)
            VALUES 
            (@fileId, @id, @title, @desc, @createdBy, @assignedTo, @status, @createdAt, @dueDate);
            SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@fileId", issue.FileId);
        cmd.Parameters.AddWithValue("@id", issue.ID);
        cmd.Parameters.AddWithValue("@title", issue.Title);
        cmd.Parameters.AddWithValue("@desc", issue.Description ?? "");
        cmd.Parameters.AddWithValue("@createdBy", issue.CreatedBy);
        cmd.Parameters.AddWithValue("@assignedTo", (object?)issue.AssignedTo ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@status", issue.Status);
        cmd.Parameters.AddWithValue("@createdAt", issue.CreatedAt);
        cmd.Parameters.AddWithValue("@dueDate", (object?)issue.DueDate ?? DBNull.Value);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<FileIssueInfo> GetAllIssues(string ID)
    {
        var list = new List<FileIssueInfo>();
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "SELECT * FROM FileIssueInfo WHERE ID = @id ORDER BY created_at DESC";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", ID);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new FileIssueInfo
            {
                IssueId = reader.GetInt32("issue_id"),
                FileId = reader.GetInt32("file_id"),
                ID = reader.GetString("ID"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                CreatedBy = reader.GetString("created_by"),
                AssignedTo = reader.IsDBNull("assigned_to") ? null : reader.GetString("assigned_to"),
                Status = reader.GetString("status"),
                CreatedAt = reader.GetDateTime("created_at"),
                DueDate = reader.IsDBNull("due_date") ? null : reader.GetDateTime("due_date")
            });
        }

        return list;
    }

    public List<FileIssueInfo> GetIssuesByFileId(int fileId)
    {
        var list = new List<FileIssueInfo>();
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "SELECT * FROM FileIssueInfo WHERE file_id = @fileId";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@fileId", fileId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new FileIssueInfo
            {
                IssueId = reader.GetInt32("issue_id"),
                FileId = reader.GetInt32("file_id"),
                ID = reader.GetString("ID"),
                Title = reader.GetString("title"),
                Description = reader.GetString("description"),
                CreatedBy = reader.GetString("created_by"),
                AssignedTo = reader.IsDBNull("assigned_to") ? null : reader.GetString("assigned_to"),
                Status = reader.GetString("status"),
                CreatedAt = reader.GetDateTime("created_at"),
                DueDate = reader.IsDBNull("due_date") ? null : reader.GetDateTime("due_date")
            });
        }

        return list;
    }

    public bool UpdateIssueStatus(int issueId, string status)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "UPDATE FileIssueInfo SET status = @status WHERE issue_id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@id", issueId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool AssignIssue(int issueId, string assignedTo)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "UPDATE FileIssueInfo SET assigned_to = @assigned WHERE issue_id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@assigned", assignedTo);
        cmd.Parameters.AddWithValue("@id", issueId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public bool DeleteIssue(int issueId)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "DELETE FROM FileIssueInfo WHERE issue_id = @id";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@id", issueId);

        return cmd.ExecuteNonQuery() > 0;
    }

    public int AddComment(FileIssueComment comment)
    {
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = @"INSERT INTO FileIssueComment 
                         (issue_id, commenter_id, content, created_at)
                         VALUES (@issueId, @commenterId, @content, @createdAt);
                         SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@issueId", comment.IssueId);
        cmd.Parameters.AddWithValue("@commenterId", comment.CommenterId);
        cmd.Parameters.AddWithValue("@content", comment.Content);
        cmd.Parameters.AddWithValue("@createdAt", comment.CreatedAt);

        return Convert.ToInt32(cmd.ExecuteScalar());
    }

    public List<FileIssueComment> GetCommentsByIssueId(int issueId)
    {
        var list = new List<FileIssueComment>();
        using var conn = new MySqlConnection(connectionString);
        conn.Open();

        string query = "SELECT * FROM FileIssueComment WHERE issue_id = @issueId ORDER BY created_at ASC";
        using var cmd = new MySqlCommand(query, conn);
        cmd.Parameters.AddWithValue("@issueId", issueId);

        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new FileIssueComment
            {
                CommentId = reader.GetInt32("comment_id"),
                IssueId = reader.GetInt32("issue_id"),
                CommenterId = reader.GetString("commenter_id"),
                Content = reader.GetString("content"),
                CreatedAt = reader.GetDateTime("created_at")
            });
        }

        return list;
    }
}
