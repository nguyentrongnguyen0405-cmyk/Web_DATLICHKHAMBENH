using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Data
{
    public static class DbInitializer
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            await context.Database.MigrateAsync();

            string[] roles = { "Doctor", "Patient", "Admin" };
            foreach (var role in roles)
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));

            // Thêm chuyên khoa nếu chưa có (có ImageUrl)
            if (!context.Specialties.Any())
            {
                context.Specialties.AddRange(
                    new Specialty { Name = "Khám tổng quát", Description = "Kiểm tra sức khỏe toàn diện", Price = "500.000đ", ShortDescription = "Gói khám tổng quát đầy đủ", ImageUrl = "/images/services/kham-tong-quat.jpg" },
                    new Specialty { Name = "Nhi khoa", Description = "Chuyên khoa nhi", Price = "400.000đ", ShortDescription = "Chăm sóc sức khỏe trẻ em", ImageUrl = "/images/services/nhi-khoa.jpg" },
                    new Specialty { Name = "Răng hàm mặt", Description = "Nha khoa thẩm mỹ", Price = "600.000đ", ShortDescription = "Dịch vụ nha khoa", ImageUrl = "/images/services/rang-ham-mat.jpg" },
                    new Specialty { Name = "Tim mạch", Description = "Bệnh lý tim mạch", Price = "700.000đ", ShortDescription = "Khám và tầm soát tim mạch", ImageUrl = "/images/services/tim-mach.jpg" },
                    new Specialty { Name = "Xét nghiệm", Description = "Xét nghiệm máu, nước tiểu", Price = "300.000đ", ShortDescription = "Trung tâm xét nghiệm", ImageUrl = "/images/services/xet-nghiem.jpg" }
                );
                await context.SaveChangesAsync();
            }

            var specialties = await context.Specialties.ToListAsync();
            var specialtyIds = specialties.ToDictionary(s => s.Name, s => s.Id);

            // Tạo bác sĩ mẫu
            await SeedDoctor(userManager, context, "doctor1@example.com", "PGS.TS.BS Nguyễn Văn An", specialtyIds.GetValueOrDefault("Khám tổng quát"), "Phó Giáo sư, Tiến sĩ Y khoa", 15, 300000);
            await SeedDoctor(userManager, context, "doctor2@example.com", "TS.BS Trần Thị Bích", specialtyIds.GetValueOrDefault("Nhi khoa"), "Tiến sĩ Y khoa", 10, 250000);

            // Tạo admin nếu chưa có
            if (!await userManager.Users.AnyAsync(u => u.Email == "admin@example.com"))
            {
                var admin = new ApplicationUser
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    FullName = "Quản trị viên",
                    EmailConfirmed = true
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Tạo lịch trực
            if (!context.Schedules.Any())
            {
                var doctors = await context.Doctors.ToListAsync();
                var startDate = DateTime.Today.AddDays(1);
                var timeSlots = new[] { "08:00", "09:00", "10:00", "13:00", "14:00", "15:00" };

                foreach (var doctor in doctors)
                {
                    for (int i = 0; i < 7; i++)
                    {
                        var workDate = startDate.AddDays(i);
                        foreach (var slot in timeSlots)
                        {
                            var startTime = TimeSpan.Parse(slot);
                            context.Schedules.Add(new Schedule
                            {
                                DoctorId = doctor.Id,
                                WorkDate = workDate,
                                StartTime = startTime,
                                EndTime = startTime.Add(TimeSpan.FromHours(1)),
                                SlotDuration = 60,
                                IsActive = true
                            });
                        }
                    }
                }
                await context.SaveChangesAsync();
            }
        }

        private static async Task SeedDoctor(UserManager<ApplicationUser> userManager, ApplicationDbContext context,
            string email, string fullName, int? specialtyId, string qualifications, int exp, decimal fee)
        {
            var user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                user = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    FullName = fullName,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(user, "Doctor123!");
                if (result.Succeeded)
                    await userManager.AddToRoleAsync(user, "Doctor");
            }

            if (user != null && specialtyId.HasValue && !context.Doctors.Any(d => d.UserId == user.Id))
            {
                context.Doctors.Add(new Doctor
                {
                    UserId = user.Id,
                    FullName = fullName,
                    SpecialtyId = specialtyId.Value,
                    Qualifications = qualifications,
                    ExperienceYears = exp,
                    ConsultationFee = fee
                });
                await context.SaveChangesAsync();
            }
        }
    }
}