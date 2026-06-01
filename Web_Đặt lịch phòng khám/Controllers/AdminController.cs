using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Dashboard - thống kê và doanh thu
        public async Task<IActionResult> Index()
        {
            ViewBag.UserCount = await _userManager.Users.CountAsync();
            ViewBag.DoctorCount = await _context.Doctors.CountAsync();
            ViewBag.AppointmentCount = await _context.Appointments.CountAsync();
            ViewBag.SpecialtyCount = await _context.Specialties.CountAsync();

            var paidAppointments = await _context.Appointments
                .Where(a => a.PaymentStatus == "Paid")
                .ToListAsync();
            ViewBag.TotalRevenue = paidAppointments.Sum(a => a.ConsultationFee ?? 0);
            ViewBag.PaidAppointmentCount = paidAppointments.Count;
            ViewBag.AverageRevenue = ViewBag.PaidAppointmentCount > 0 ? ViewBag.TotalRevenue / ViewBag.PaidAppointmentCount : 0;

            var topDoctor = await _context.Appointments
                .GroupBy(a => a.DoctorId)
                .Select(g => new { DoctorId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            if (topDoctor != null)
            {
                var doctor = await _context.Doctors.FindAsync(topDoctor.DoctorId);
                ViewBag.TopDoctor = doctor?.FullName ?? "Chưa có dữ liệu";
                ViewBag.TopDoctorCount = topDoctor.Count;
            }
            else
            {
                ViewBag.TopDoctor = "Chưa có dữ liệu";
                ViewBag.TopDoctorCount = 0;
            }

            var topConsultant = await _userManager.Users
                .OrderByDescending(u => u.CreatedAt)
                .FirstOrDefaultAsync();
            ViewBag.TopConsultant = topConsultant?.FullName ?? "Chưa có dữ liệu";

            return View();
        }

        // Quản lý người dùng
        public async Task<IActionResult> Users()
        {
            var users = await _userManager.Users.ToListAsync();
            var model = new List<UserRoleViewModel>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                model.Add(new UserRoleViewModel { User = user, Roles = roles.ToList() });
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null && await _roleManager.RoleExistsAsync(role))
            {
                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, role);
                TempData["Success"] = $"Đã gán role '{role}' cho người dùng {user.Email}.";
            }
            else
            {
                TempData["Error"] = "Không tìm thấy người dùng hoặc role không hợp lệ.";
            }
            return RedirectToAction("Users");
        }

        // Quản lý chuyên khoa
        public async Task<IActionResult> Specialties()
        {
            return View(await _context.Specialties.ToListAsync());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddSpecialty(Specialty specialty)
        {
            if (ModelState.IsValid)
            {
                _context.Specialties.Add(specialty);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Thêm chuyên khoa thành công!";
            }
            return RedirectToAction("Specialties");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSpecialty(int id)
        {
            var specialty = await _context.Specialties.FindAsync(id);
            if (specialty != null)
            {
                _context.Specialties.Remove(specialty);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa chuyên khoa thành công!";
            }
            return RedirectToAction("Specialties");
        }

        // Quản lý bác sĩ
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .ToListAsync();
            return View(doctors);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDoctor(int id)
        {
            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor != null)
            {
                _context.Doctors.Remove(doctor);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Xóa bác sĩ thành công!";
            }
            return RedirectToAction("Doctors");
        }

        // Thêm bác sĩ mới
        public async Task<IActionResult> CreateDoctor()
        {
            ViewBag.Specialties = await _context.Specialties.ToListAsync();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDoctor(string email, string fullName, int specialtyId, string qualifications, int experienceYears, decimal consultationFee)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(fullName))
            {
                ModelState.AddModelError("", "Email và họ tên không được để trống.");
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View();
            }

            var existingUser = await _userManager.FindByEmailAsync(email);
            if (existingUser != null)
            {
                ModelState.AddModelError("", "Email đã được sử dụng.");
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View();
            }

            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAt = DateTime.Now,
                IsActive = true
            };
            var password = "Doctor@123";
            var result = await _userManager.CreateAsync(user, password);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                    ModelState.AddModelError("", error.Description);
                ViewBag.Specialties = await _context.Specialties.ToListAsync();
                return View();
            }

            await _userManager.AddToRoleAsync(user, "Doctor");

            var doctor = new Doctor
            {
                UserId = user.Id,
                FullName = fullName,
                SpecialtyId = specialtyId,
                Qualifications = qualifications,
                ExperienceYears = experienceYears,
                ConsultationFee = consultationFee
            };
            _context.Doctors.Add(doctor);
            await _context.SaveChangesAsync();

            TempData["Success"] = $"Bác sĩ {fullName} đã được thêm thành công. Email: {email}, Mật khẩu: {password}";
            return RedirectToAction("Doctors");
        }

        // DOANH THU - chi tiết
        public async Task<IActionResult> Revenue()
        {
            var paidAppointments = await _context.Appointments
                .Where(a => a.PaymentStatus == "Paid")
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .Include(a => a.Schedule)
                .ToListAsync();

            ViewBag.TotalRevenue = paidAppointments.Sum(a => a.ConsultationFee ?? 0);

            // Doanh thu theo giờ
            var hourlyRevenue = paidAppointments
                .Where(a => a.Schedule != null)
                .GroupBy(a => a.Schedule.StartTime.Hours)
                .Select(g => new { Hour = g.Key, Total = g.Sum(x => x.ConsultationFee ?? 0), Count = g.Count() })
                .OrderBy(g => g.Hour)
                .ToList();
            ViewBag.HourlyRevenue = hourlyRevenue;

            // Doanh thu theo ngày
            var dailyRevenue = paidAppointments
                .Where(a => a.Schedule != null)
                .GroupBy(a => a.Schedule.WorkDate.Date)
                .Select(g => new { Date = g.Key, Total = g.Sum(x => x.ConsultationFee ?? 0), Count = g.Count() })
                .OrderByDescending(g => g.Date)
                .ToList();
            ViewBag.DailyRevenue = dailyRevenue;

            ViewBag.PaidAppointments = paidAppointments;
            return View();
        }
    }

    public class UserRoleViewModel
    {
        public ApplicationUser User { get; set; }
        public List<string> Roles { get; set; }
    }
}