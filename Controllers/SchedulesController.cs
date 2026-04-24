using Microsoft.AspNetCore.Authorization;
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
        public SchedulesController(ApplicationDbContext context) => _context = context;

        public async Task<IActionResult> Index()
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            if (user == null) return NotFound();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound();

            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctor.Id && s.WorkDate >= DateTime.Today)
                .OrderBy(s => s.WorkDate).ThenBy(s => s.StartTime)
                .ToListAsync();
            return View(schedules);
        }

        public IActionResult Create() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Schedule schedule)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.UserName == User.Identity!.Name);
            if (user == null) return NotFound();
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound();

            schedule.DoctorId = doctor.Id;
            schedule.IsActive = true;
            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}