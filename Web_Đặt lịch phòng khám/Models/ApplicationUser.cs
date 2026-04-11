#nullable enable
using Microsoft.AspNetCore.Identity;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public string? AvatarUrl { get; set; }
        public DateTime? CreatedAt { get; set; }
        public bool IsActive { get; set; } = true;
    }
}