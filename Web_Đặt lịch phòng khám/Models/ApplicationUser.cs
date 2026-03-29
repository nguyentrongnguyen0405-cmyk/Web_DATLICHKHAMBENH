using Microsoft.AspNetCore.Identity;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}