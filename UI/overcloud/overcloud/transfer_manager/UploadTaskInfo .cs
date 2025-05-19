using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace overcloud.transfer_manager
{
    public class UploadTaskInfo
    {
        public string FileName { get; set; } // 실제 파일 이름 (UI 표시용)
        public string LocalPath { get; set; } // 로컬 파일 경로
        public int FolderId { get; set; }     // 업로드할 클라우드 폴더 ID
    }
}
