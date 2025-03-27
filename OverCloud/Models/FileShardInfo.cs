using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OverCloud.Models
{
    public class FileShardInfo
    {
        public string FileName { get; set; }           // 원본 파일명
        public int PartIndex { get; set; }             // 몇 번째 조각인지
        public long Size { get; set; }                 // 조각 크기 (bytes)
        public string AssignedAccountId { get; set; }  // 어떤 계정에 저장될지
        public string LocalPath { get; set; }          // 임시 저장된 파일 경로
    }

}
