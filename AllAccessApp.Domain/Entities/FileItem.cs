using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;

namespace AllAccessApp.Domain.Entities
{
    public class FileItem: BaseEntity
    {
        [Required]
        public string FileName { get; set; } = string.Empty;
        [Required]
        public string OriginalName { get; set; } = string.Empty;
        [Required]
        public string FileType {  get; set; } = string.Empty ;
        public long FileSize { get; set; }
        public long OriginalFileSize { get; set; } // Size before compression
        public bool IsCompressed { get; set; } // Whether file was compressed
        [Required]
        public string FilePath { get; set; }=string.Empty ;
        
        public int UserId { get; set; }
        public virtual User User { get; set; } = null!;
        

    }
}
