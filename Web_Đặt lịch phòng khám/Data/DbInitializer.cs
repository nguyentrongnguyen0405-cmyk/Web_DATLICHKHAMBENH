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

            // Tạo database nếu chưa có
            await context.Database.MigrateAsync();

            // Tạo role
            if (!await roleManager.RoleExistsAsync("Doctor"))
                await roleManager.CreateAsync(new IdentityRole("Doctor"));
            if (!await roleManager.RoleExistsAsync("Patient"))
                await roleManager.CreateAsync(new IdentityRole("Patient"));

            // ========== 1. CHUYÊN KHOA ==========
            if (!context.Specialties.Any())
            {
                context.Specialties.AddRange(
                    new Specialty { Name = "Nội tổng quát", Description = "Khám nội khoa" },
                    new Specialty { Name = "Nhi khoa", Description = "Khám trẻ em" }
                );
                await context.SaveChangesAsync();
            }

            // ========== 2. BỆNH NHÂN ==========
            var patientEmail = "patient@example.com";
            if (await userManager.FindByEmailAsync(patientEmail) == null)
            {
                var patient = new ApplicationUser
                {
                    UserName = patientEmail,
                    Email = patientEmail,
                    FullName = "Nguyễn Văn Bệnh",
                    Role = "Patient",
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(patient, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(patient, "Patient");
                }
            }

            // ========== 3. TẠO BÁC SĨ ==========
            // Bác sĩ 1
            var doctor1Email = "doctor1@example.com";
            var doctor1 = await userManager.FindByEmailAsync(doctor1Email);
            if (doctor1 == null)
            {
                doctor1 = new ApplicationUser
                {
                    UserName = doctor1Email,
                    Email = doctor1Email,
                    FullName = "PGS.TS.BS Nguyễn Văn An",
                    Role = "Doctor",
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(doctor1, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctor1, "Doctor");
                }
            }

            // Bác sĩ 2
            var doctor2Email = "doctor2@example.com";
            var doctor2 = await userManager.FindByEmailAsync(doctor2Email);
            if (doctor2 == null)
            {
                doctor2 = new ApplicationUser
                {
                    UserName = doctor2Email,
                    Email = doctor2Email,
                    FullName = "TS.BS Trần Thị Bích",
                    Role = "Doctor",
                    CreatedAt = DateTime.Now,
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(doctor2, "123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(doctor2, "Doctor");
                }
            }

            // Lưu user vào database trước khi thêm Doctors
            await context.SaveChangesAsync();

            // ========== 4. THÊM DOCTORS ==========
            // Lấy lại user từ database để có Id
            var savedDoctor1 = await userManager.FindByEmailAsync(doctor1Email);
            var savedDoctor2 = await userManager.FindByEmailAsync(doctor2Email);
            var specialty1 = await context.Specialties.FirstOrDefaultAsync(s => s.Name == "Nội tổng quát");
            var specialty2 = await context.Specialties.FirstOrDefaultAsync(s => s.Name == "Nhi khoa");

            if (!context.Doctors.Any())
            {
                if (specialty1 != null && savedDoctor1 != null)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = savedDoctor1.Id,
                        SpecialtyId = specialty1.Id,
                        Qualification = "Phó Giáo sư, Tiến sĩ Y khoa",
                        ExperienceYears = 15
                    });
                }

                if (specialty2 != null && savedDoctor2 != null)
                {
                    context.Doctors.Add(new Doctor
                    {
                        UserId = savedDoctor2.Id,
                        SpecialtyId = specialty2.Id,
                        Qualification = "Tiến sĩ Y khoa",
                        ExperienceYears = 12
                    });
                }
                await context.SaveChangesAsync();
            }

            // ========== 5. LỊCH LÀM VIỆC ==========
            var doctors = context.Doctors.ToList();
            var today = DateTime.Today;

            if (!context.Schedules.Any() && doctors.Any())
            {
                foreach (var doc in doctors)
                {
                    // Ngày hôm nay
                    context.Schedules.Add(new Schedule
                    {
                        DoctorId = doc.Id,
                        WorkDate = today,
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(12),
                        SlotDuration = 30,
                        IsActive = true
                    });

                    // Ngày mai
                    context.Schedules.Add(new Schedule
                    {
                        DoctorId = doc.Id,
                        WorkDate = today.AddDays(1),
                        StartTime = TimeSpan.FromHours(8),
                        EndTime = TimeSpan.FromHours(12),
                        SlotDuration = 30,
                        IsActive = true
                    });
                }
                await context.SaveChangesAsync();
            }
        }
    }
}