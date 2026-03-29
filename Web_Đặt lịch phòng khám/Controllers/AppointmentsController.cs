using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /Appointments/Create
        public async Task<IActionResult> Create()
        {
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
        public async Task<IActionResult> Create(Appointment appointment, DateTime WorkDate)
        {
            ModelState.Remove("PatientId");
            ModelState.Remove("CreatedAt");
            ModelState.Remove("Status");
            ModelState.Remove("ScheduleId");

            if (ModelState.IsValid)
            {
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId &&
                                               s.WorkDate.Date == WorkDate.Date &&
                                               s.IsActive);
                if (schedule == null)
                {
                    TempData["Error"] = "Bác sĩ không làm việc ngày này. Vui lòng chọn ngày khác.";
                    return RedirectToAction("Create");
                }

                var isBooked = await _context.Appointments
                    .AnyAsync(a => a.DoctorId == appointment.DoctorId &&
                                   a.ScheduleId == schedule.Id &&
                                   a.AppointmentTime == appointment.AppointmentTime &&
                                   (a.Status == "pending" || a.Status == "confirmed"));
                if (isBooked)
                {
                    TempData["Error"] = "Khung giờ này đã được đặt. Vui lòng chọn giờ khác.";
                    return RedirectToAction("Create");
                }

                var userId = _userManager.GetUserId(User);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Bạn cần đăng nhập để đặt lịch.";
                    return RedirectToAction("Create");
                }

                appointment.ScheduleId = schedule.Id;
                appointment.PatientId = userId;
                appointment.Status = "confirmed";  // Đã sửa: tự động xác nhận
                appointment.CreatedAt = DateTime.Now;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đặt lịch thành công! Lịch hẹn của bạn đã được xác nhận.";
                return RedirectToAction("MyAppointments");  // Đã sửa: chuyển về trang lịch hẹn
            }

            var doctors = await _context.Doctors.Include(d => d.User).Include(d => d.Specialty).ToListAsync();
            ViewBag.Doctors = doctors;
            return View(appointment);
        }

        // AJAX: Lấy slot trống
        [HttpGet]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, DateTime date)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.DoctorId == doctorId && s.WorkDate.Date == date.Date && s.IsActive);

            if (schedule == null)
                return Json(new { slots = new List<string>() });

            var slots = new List<string>();
            var start = schedule.StartTime;
            var end = schedule.EndTime;
            var duration = schedule.SlotDuration;

            var current = start;
            while (current < end)
            {
                slots.Add(current.ToString(@"hh\:mm"));
                current = current.Add(TimeSpan.FromMinutes(duration));
            }

            var bookedSlots = await _context.Appointments
                .Where(a => a.DoctorId == doctorId && a.ScheduleId == schedule.Id &&
                            (a.Status == "pending" || a.Status == "confirmed"))
                .Select(a => a.AppointmentTime.ToString(@"hh\:mm"))
                .ToListAsync();

            var availableSlots = slots.Except(bookedSlots).ToList();
            return Json(new { slots = availableSlots });
        }

        // GET: /Appointments/MyAppointments
        [Authorize]
        public async Task<IActionResult> MyAppointments()
        {
            var userId = _userManager.GetUserId(User);
            var appointments = await _context.Appointments
                .Include(a => a.Doctor)
                    .ThenInclude(d => d.User)
                .Include(a => a.Doctor.Specialty)
                .Include(a => a.Schedule)
                .Where(a => a.PatientId == userId)
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return View(appointments);
        }

        // POST: /Appointments/Cancel
        [Authorize]
        public async Task<IActionResult> Cancel(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment != null && appointment.PatientId == _userManager.GetUserId(User))
            {
                appointment.Status = "cancelled";
                await _context.SaveChangesAsync();
                TempData["Success"] = "Đã hủy lịch hẹn thành công.";
            }
            return RedirectToAction("MyAppointments");
        }
    }
}