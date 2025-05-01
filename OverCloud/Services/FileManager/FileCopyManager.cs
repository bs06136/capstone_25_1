using System;
using DB.overcloud.Models;
using DB.overcloud.Repository;
using System.Threading.Tasks;

namespace OverCloud.Services.FileManager
{
    public class FileCopyManager
    {
        private readonly IFileRepository fileRepository;

        public FileCopyManager(IFileRepository fileRepo)
        {
            fileRepository = fileRepo;
        }

        public async Task<bool> Copy_File(int copy_target_file_id, int target_parent_file_Id)
        {
            // 1. 복사할 파일 정보 조회
            var originalFile = fileRepository.GetFileById(copy_target_file_id);
            if (originalFile == null)
            {
                Console.WriteLine("❌ 복사할 원본 파일을 찾을 수 없습니다.");
                return false;
            }

            // 2. 복사할 파일 정보 생성
            CloudFileInfo copiedFile = new CloudFileInfo
            {
                FileName = GenerateCopiedFileName(originalFile.FileName),
                FileSize = originalFile.FileSize/(1024),
                UploadedAt = DateTime.Now,
                CloudStorageNum = originalFile.CloudStorageNum,
                ParentFolderId = target_parent_file_Id,
                IsFolder = originalFile.IsFolder,
                Count = 0,
            };

            // 3. DB에 복제 파일 저장
            bool result = fileRepository.addfile(copiedFile);

            if (result)
            {
                Console.WriteLine("✅ 파일 복사 성공");
            }
            else
            {
                Console.WriteLine("❌ 파일 복사 실패");
            }

            return result;
        }

        private string GenerateCopiedFileName(string originalFileName)
        {
            // 파일명에 (복사본) 추가하는 로직
            var extension = System.IO.Path.GetExtension(originalFileName);
            var nameWithoutExtension = System.IO.Path.GetFileNameWithoutExtension(originalFileName);
            return $"{nameWithoutExtension}(복사본){extension}";
        }
    }
}
