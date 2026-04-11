using System.ComponentModel.DataAnnotations;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Specialty
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string? Price { get; set; }
        public string? ShortDescription { get; set; }
        public string? FullDescription { get; set; }
        public string? Features { get; set; }
        public string? Tests { get; set; }
        public string? Benefits { get; set; }
        public virtual ICollection<Doctor>? Doctors { get; set; }
    }
}