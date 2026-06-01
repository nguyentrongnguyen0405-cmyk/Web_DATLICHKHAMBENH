using System.ComponentModel.DataAnnotations;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Medicine
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public string? Description { get; set; }
    }
}