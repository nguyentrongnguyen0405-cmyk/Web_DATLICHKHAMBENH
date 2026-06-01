using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize(Roles = "Doctor")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public DoctorController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null) return NotFound();

            // Thống kê
            ViewBag.AppointmentCount = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id);
            ViewBag.PendingCount = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id && a.Status == "pending");
            ViewBag.ConfirmedCount = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id && a.Status == "confirmed");
            ViewBag.CancelledCount = await _context.Appointments.CountAsync(a => a.DoctorId == doctor.Id && a.Status == "cancelled");

            // Top bệnh nhân đặt lịch nhiều nhất với bác sĩ này
            var topPatient = await _context.Appointments
                .Where(a => a.DoctorId == doctor.Id)
                .GroupBy(a => a.UserId)
                .Select(g => new { UserId = g.Key, Count = g.Count() })
                .OrderByDescending(x => x.Count)
                .FirstOrDefaultAsync();
            if (topPatient != null)
            {
                var patient = await _userManager.FindByIdAsync(topPatient.UserId);
                ViewBag.TopPatient = patient?.FullName ?? "Khách";
                ViewBag.TopPatientCount = topPatient.Count;
            }
            else
            {
                ViewBag.TopPatient = "Chưa có dữ liệu";
                ViewBag.TopPatientCount = 0;
            }

            return View();
        }
    }
}