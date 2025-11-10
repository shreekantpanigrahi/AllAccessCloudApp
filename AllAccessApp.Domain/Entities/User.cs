using System.ComponentModel.DataAnnotations;

namespace AllAccessApp.Domain.Entities
{
    public class User:BaseEntity
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        [Required]
        public string PasswordHash { get; set; }= string.Empty;
        [Required]
        public string Name { get; set; } = string.Empty;
        [Required]
        public long UsedStorage { get; set; } = 0;
        [Required]
        public long StorageQuota { get; set; } = 524_288_000; // 500 MB
        public string? ProfilePictureUrl { get; set; } 
        public string? ResetPasswordOtp { get; set; }  // Hashed OTP
        public DateTime? ResetPasswordOtpExpiry { get;set; } // When it expires
        public virtual ICollection<FileItem> Files { get; set; } = new List<FileItem>();
    }
}
