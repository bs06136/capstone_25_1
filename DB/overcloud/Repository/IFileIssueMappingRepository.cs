using DB.overcloud.Models;
using System.Collections.Generic;

namespace DB.overcloud.Repository
{
    public interface IFileIssueMappingRepository
    {
        bool AddMapping(int issueId, int fileId);
        List<int> GetFileIdsByIssueId(int issueId);
        List<int> GetIssueIdsByFileId(int fileId);
        bool DeleteMapping(int issueId, int fileId);
        bool DeleteMappingsByIssueId(int issueId);
        bool DeleteMappingsByFileId(int fileId);
    }
}