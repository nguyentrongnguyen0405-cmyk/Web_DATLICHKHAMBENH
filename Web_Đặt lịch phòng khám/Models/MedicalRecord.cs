using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Web_Đặt_lịch_phòng_khám.Models
{
    public class MedicalRecord
    {
        [Key]
        public int Id { get; set; }

        public int AppointmentId { get; set; }
        [ForeignKey("AppointmentId")]
        public virtual Appointment? Appointment { get; set; }

        public string? Diagnosis { get; set; }          // Chẩn đoán
        public string? ExaminationResult { get; set; } // Kết quả khám
        public string? Notes { get; set; }              // Ghi chú
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation property cho danh sách đơn thuốc
        public virtual ICollection<Prescription>? Prescriptions { get; set; }
    }
}