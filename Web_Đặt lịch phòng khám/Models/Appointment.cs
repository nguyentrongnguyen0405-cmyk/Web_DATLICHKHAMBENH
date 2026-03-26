#nullable disable
using System;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public int PatientId { get; set; }
        public User Patient { get; set; }
        public int DoctorId { get; set; }
        public Doctor Doctor { get; set; }
        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }
        public TimeSpan AppointmentTime { get; set; }
        public string Symptoms { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}