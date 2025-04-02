using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using OverCloud.Models;
using OverCloud.Services;

namespace OverCloud.Services
{
    public class FileService
    {
        private readonly IFileRepository fileRepo;

        public FileService(IFileRepository repo)
        {
            this.fileRepo = repo;
        }

        public List<CloudFileInfo> GetAllFiles() => fileRepo.GetAllFileInfo();

      //  public CloudFileInfo GetFile(int fileId) => fileRepo.GetFileById(fileId);

        public bool SaveFile(CloudFileInfo file) => fileRepo.AddFile(file);

        public bool RemoveFile(int fileId) => fileRepo.DeleteFile(fileId);
    }
}