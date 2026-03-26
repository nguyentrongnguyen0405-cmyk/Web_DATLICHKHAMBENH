namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Specialty
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ICollection<Doctor> Doctors { get; set; }
    }
}
