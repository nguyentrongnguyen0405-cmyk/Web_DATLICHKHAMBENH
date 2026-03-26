using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Specialty> Specialties { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Schedule> Schedules { get; set; }
        public DbSet<Appointment> Appointments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ cho Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany()  // Nếu User không có collection Appointments, để WithMany()
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Schedule)
                .WithMany()  // Nếu Schedule không có collection Appointments, để WithMany()
                .HasForeignKey(a => a.ScheduleId)
                .OnDelete(DeleteBehavior.Restrict); // Không cascade

            // Nếu bạn muốn giữ cascade cho một trong các quan hệ, hãy chọn chỉ một.
            // Ví dụ: chỉ cascade khi xóa Doctor, thì các Appointment liên quan bị xóa:
            // .OnDelete(DeleteBehavior.Cascade)
        }
    }
}