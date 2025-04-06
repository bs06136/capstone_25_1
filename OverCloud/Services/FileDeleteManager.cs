﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DB.overcloud.Models;
using DB.overcloud.Service;
using DB.overcloud.Models;

namespace OverCloud.Services
{
    public class FileDeleteManager
    {
        private readonly FileService fileService;

        public FileDeleteManager(FileService fileService)
        {
            this.fileService = fileService;
        }

        // 파일 ID를 기반으로 삭제
        public bool DeleteFileById(int fileId)
        {
            var file = fileService.GetFile(fileId);
            if (file == null)
            {
                Console.WriteLine("❌ 삭제할 파일이 존재하지 않습니다.");
                return false;
            }

            return fileService.RemoveFile(fileId);
        }

        //// (선택) 폴더 전체 삭제 등 확장 가능
        //public bool DeleteFolderRecursively(int folderId)
        //{
        //    // 하위 파일, 폴더 탐색 및 재귀적 삭제 로직 추가 가능
        //    // (이건 추후 구현)
        //    return false;
        //}
    }


}