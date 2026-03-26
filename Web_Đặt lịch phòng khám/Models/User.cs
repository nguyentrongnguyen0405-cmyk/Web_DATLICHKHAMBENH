#nullable disable
using System.ComponentModel.DataAnnotations;
namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required, EmailAddress]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}