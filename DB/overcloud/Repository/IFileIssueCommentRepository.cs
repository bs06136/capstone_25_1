using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IFileIssueCommentRepository
    {
        bool AddComment(FileIssueComment comment);
        List<FileIssueComment> GetCommentsByIssueId(int issueId);
        bool DeleteComment(int commentId);
        FileIssueComment? GetCommentById(int commentId);
        bool UpdateComment(int commentId, string newContent);
    }
}
