#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Appointments/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d!.Specialty)
                .Include(a => a.Schedule)
                .Include(a => a.User); // Lấy thông tin bệnh nhân cho bác sĩ

            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor == null) return NotFound("Không tìm thấy thông tin bác sĩ.");
                query = query.Where(a => a.DoctorId == doctor.Id);
                ViewBag.IsDoctor = true;
                ViewBag.Title = "Danh sách bệnh nhân đặt lịch";
            }
            else
            {
                query = query.Where(a => a.UserId == user.Id);
                ViewBag.IsDoctor = false;
                ViewBag.Title = "Lịch hẹn của tôi";
            }

            var appointments = await query.OrderByDescending(a => a.Schedule!.WorkDate).ToListAsync();
            return View(appointments);
        }

        // POST: /Appointments/UpdateStatus
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            // Kiểm tra bác sĩ có quyền với lịch hẹn này không
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return Forbid();

            if (status == "confirmed" || status == "cancelled")
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật trạng thái thành {(status == "confirmed" ? "Đã xác nhận" : "Đã hủy")}.";
            }
            else
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
            }
            return RedirectToAction(nameof(MyAppointments));
        }

        // GET: /Appointments/Create
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .ToListAsync();

            ViewBag.Doctors = doctors;
            return View();
        }

        // POST: /Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(int doctorId, DateTime workDate, string appointmentTime, string? symptoms)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!TimeSpan.TryParse(appointmentTime, out var startTime))
            {
                ModelState.AddModelError("", "Giờ khám không hợp lệ.");
                await LoadDoctorsToViewBag();
                return View();
            }

            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.WorkDate.Date == workDate.Date && s.StartTime == startTime && s.IsActive == true);

            if (schedule == null)
            {
                ModelState.AddModelError("", "Khung giờ không khả dụng.");
                await LoadDoctorsToViewBag();
                return View();
            }

            // Kiểm tra xem khung giờ đã được đặt chưa
            var existingAppointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.ScheduleId == schedule.Id);
            if (existingAppointment != null)
            {
                ModelState.AddModelError("", "Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.");
                await LoadDoctorsToViewBag();
                return View();
            }

            var appointment = new Appointment
            {
                UserId = user.Id,
                DoctorId = doctorId,
                ScheduleId = schedule.Id,
                Symptoms = symptoms ?? string.Empty,
                Status = "pending",
                CreatedAt = DateTime.Now
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đặt lịch thành công!";
            return RedirectToAction(nameof(MyAppointments));
        }

        // AJAX: /Appointments/GetAvailableSlots
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctorId && s.WorkDate.Date == date.Date && s.IsActive == true)
                .ToListAsync();

            var bookedScheduleIds = await _context.Appointments
                .Where(a => a.Schedule != null && a.Schedule.DoctorId == doctorId && a.Schedule.WorkDate.Date == date.Date)
                .Select(a => a.ScheduleId)
                .ToListAsync();

            var availableSlots = schedules
                .Where(s => !bookedScheduleIds.Contains(s.Id))
                .Select(s => s.StartTime.ToString(@"hh\:mm"))
                .OrderBy(t => t)
                .ToList();

            return Json(new { slots = availableSlots });
        }

        private async Task LoadDoctorsToViewBag()
        {
            var doctors = await _context.Doctors
                .Include(d => d.User)
                .Include(d => d.Specialty)
                .ToListAsync();
            ViewBag.Doctors = doctors;
        }
    }
}