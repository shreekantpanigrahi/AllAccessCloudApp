using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AllAccessApp.Application.DTOs
{
    public class FileListResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public List<FileItemDto>? Files { get; set; }

    }
}
