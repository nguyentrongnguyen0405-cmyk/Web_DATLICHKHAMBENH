using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;
        [ForeignKey("UserId")]
        public virtual ApplicationUser? User { get; set; }

        public int DoctorId { get; set; }
        [ForeignKey("DoctorId")]
        public virtual Doctor? Doctor { get; set; } // Để gọi @item.Doctor.FullName

        public int ScheduleId { get; set; }
        [ForeignKey("ScheduleId")]
        public virtual Schedule? Schedule { get; set; }

        public string Symptoms { get; set; } = string.Empty;
        public string Status { get; set; } = "pending"; // pending, confirmed, cancelled
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}