using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Schedule(int doctorId)
        {
            var doctor = await _context.Doctors
                .Include(d => d.Specialty)
                .FirstOrDefaultAsync(d => d.Id == doctorId);

            if (doctor == null) return NotFound();

            // Sửa lỗi: Phải là _context.Schedules (có chữ 's')
            var schedules = await _context.Schedules
                .Where(s => s.DoctorId == doctorId && s.IsActive && s.WorkDate >= DateTime.Today)
                .OrderBy(s => s.WorkDate)
                .ToListAsync();

            ViewBag.Doctor = doctor;
            return View(schedules);
        }
    }
}