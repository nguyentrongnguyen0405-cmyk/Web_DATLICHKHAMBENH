using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class SchedulesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public SchedulesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Hiển thị lịch tuần
        public async Task<IActionResult> Index(DateTime? currentWeekStart)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound("Không tìm thấy bác sĩ.");

            // Xác định ngày bắt đầu tuần (Thứ Hai)
            var today = DateTime.Today;
            var startOfWeek = currentWeekStart ?? today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
            // Nếu currentWeekStart null thì lấy tuần hiện tại
            if (currentWeekStart != null)
                startOfWeek = currentWeekStart.Value.Date;
            else
                startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);

            var endOfWeek = startOfWeek.AddDays(6); // Chủ Nhật

            // Lấy tất cả lịch trực trong tuần
            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctor.Id && s.WorkDate >= startOfWeek && s.WorkDate <= endOfWeek)
                .OrderBy(s => s.WorkDate).ThenBy(s => s.StartTime)
                .ToListAsync();

            // Lấy số lượng bệnh nhân đã đặt cho mỗi schedule
            var appointmentCounts = await _context.Appointments
                .Where(a => a.Schedule != null && a.Schedule.DoctorId == doctor.Id)
                .GroupBy(a => a.ScheduleId)
                .Select(g => new { ScheduleId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(k => k.ScheduleId, v => v.Count);

            // Tạo một mảng chứa 7 ngày trong tuần
            var weekDays = Enumerable.Range(0, 7).Select(i => startOfWeek.AddDays(i)).ToList();
            // Tập hợp các khung giờ có trong tuần (lấy từ schedules, có thể mở rộng thêm giờ mặc định)
            var timeSlots = new List<TimeSpan> { new TimeSpan(8,0,0), new TimeSpan(9,0,0), new TimeSpan(10,0,0),
                                                  new TimeSpan(13,0,0), new TimeSpan(14,0,0), new TimeSpan(15,0,0) };

            ViewBag.WeekDays = weekDays;
            ViewBag.TimeSlots = timeSlots;
            ViewBag.Schedules = schedules.ToDictionary(s => (s.WorkDate, s.StartTime));
            ViewBag.AppointmentCounts = appointmentCounts;
            ViewBag.DoctorId = doctor.Id;
            ViewBag.CurrentWeekStart = startOfWeek;
            ViewBag.PrevWeek = startOfWeek.AddDays(-7);
            ViewBag.NextWeek = startOfWeek.AddDays(7);
            return View();
        }

        // POST: Thêm hoặc cập nhật lịch trực cho một khung giờ cụ thể
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSlot(DateTime workDate, TimeSpan startTime, TimeSpan endTime, int maxPatients)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound();

            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctor.Id && s.WorkDate.Date == workDate.Date && s.StartTime == startTime);

            if (schedule != null)
            {
                // Cập nhật
                schedule.MaxPatients = maxPatients;
                schedule.EndTime = endTime;
                schedule.IsActive = true;
            }
            else
            {
                // Thêm mới
                schedule = new Schedule
                {
                    DoctorId = doctor.Id,
                    WorkDate = workDate,
                    StartTime = startTime,
                    EndTime = endTime,
                    MaxPatients = maxPatients,
                    IsActive = true,
                    SlotDuration = (int)(endTime - startTime).TotalMinutes
                };
                _context.Schedules.Add(schedule);
            }
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã lưu lịch trực.";
            return RedirectToAction(nameof(Index), new { currentWeekStart = workDate.AddDays(-(int)workDate.DayOfWeek + (int)DayOfWeek.Monday) });
        }

        // POST: Xóa lịch trực
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSlot(int id, DateTime workDate)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null) return NotFound();

            var hasAppointments = await _context.Appointments.AnyAsync(a => a.ScheduleId == id);
            if (hasAppointments)
            {
                TempData["Error"] = "Không thể xóa vì đã có bệnh nhân đặt lịch.";
                return RedirectToAction(nameof(Index), new { currentWeekStart = workDate.AddDays(-(int)workDate.DayOfWeek + (int)DayOfWeek.Monday) });
            }

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa lịch trực.";
            return RedirectToAction(nameof(Index), new { currentWeekStart = workDate.AddDays(-(int)workDate.DayOfWeek + (int)DayOfWeek.Monday) });
        }
    }
}