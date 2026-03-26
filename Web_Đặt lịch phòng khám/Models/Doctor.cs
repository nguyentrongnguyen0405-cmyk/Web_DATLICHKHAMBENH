namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
        public int? SpecialtyId { get; set; }
        public Specialty Specialty { get; set; }
        public string Qualification { get; set; }
        public int ExperienceYears { get; set; }
        public ICollection<Schedule> Schedules { get; set; }
        public ICollection<Appointment> Appointments { get; set; }
    }
}