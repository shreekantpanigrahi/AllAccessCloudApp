using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.DTOs
{
    public class FileUploadResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public string? FileName { get; set; }
        public string? FilePath { get; set; }
        public long FileSize { get; set; }
        public long OriginalFileSize { get; set; }
        public string? FileType { get; set; }
        public int FileId {  get; set; }
        public bool IsCompressed { get; set; }
        public float CompressionRatio { get; set; }
    }
}
