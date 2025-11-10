using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.DTOs
{
    public class FileItemDto
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string OriginalName { get; set; } = string.Empty;
        public string FileType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public long OriginalFileSize { get; set; } 
        public bool IsCompressed { get; set; }
        public DateTime CreatedOn { get; set; }
        public DateTime ModifiedOn { get; set; }
        public string FormattedSize { get; set; } = string.Empty;
        public string FormattedOriginalSize { get; set; } = string.Empty;
    }
}
