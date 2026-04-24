using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Doctor
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty; // Sửa lỗi CS1061 FullName

        public string Qualifications { get; set; } = string.Empty; // Sửa lỗi CS1061 Qualifications

        public int ExperienceYears { get; set; }

        public decimal ConsultationFee { get; set; } // Sửa lỗi ConsultationFee

        public int SpecialtyId { get; set; }

        [ForeignKey("SpecialtyId")]
        public virtual Specialty? Specialty { get; set; }

        public string? UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }
    }
}