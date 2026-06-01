using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class Prescription
    {
        [Key]
        public int Id { get; set; }

        public int MedicalRecordId { get; set; }
        [ForeignKey("MedicalRecordId")]
        public virtual MedicalRecord? MedicalRecord { get; set; }

        public int MedicineId { get; set; }
        [ForeignKey("MedicineId")]
        public virtual Medicine? Medicine { get; set; }

        public string? Dosage { get; set; }
        public string? Instructions { get; set; }
        public int Quantity { get; set; }
        public string? Unit { get; set; }
        public decimal Price { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}