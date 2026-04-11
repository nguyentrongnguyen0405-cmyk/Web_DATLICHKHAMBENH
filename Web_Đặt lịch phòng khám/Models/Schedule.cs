using System.ComponentModel.DataAnnotations;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Schedule
    {
        [Key]
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public virtual Doctor? Doctor { get; set; }
        public DateTime WorkDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int SlotDuration { get; set; } = 60;
        public bool IsActive { get; set; } = true;
    }
}