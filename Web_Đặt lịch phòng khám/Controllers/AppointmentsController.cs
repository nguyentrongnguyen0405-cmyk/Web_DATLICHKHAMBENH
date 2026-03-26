using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AppointmentsController(ApplicationDbContext context)
        {
            _context = context;
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
                // 1. Tìm schedule theo doctorId và WorkDate
                var schedule = await _context.Schedules
                    .FirstOrDefaultAsync(s => s.DoctorId == appointment.DoctorId &&
                                               s.WorkDate.Date == WorkDate.Date &&
                                               s.IsActive);
                if (schedule == null)
                {
                    TempData["Error"] = "Bác sĩ không làm việc ngày này. Vui lòng chọn ngày khác.";
                    return RedirectToAction("Create");
                }

                // 2. Kiểm tra trùng slot
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

                // 3. Gán các giá trị
                appointment.ScheduleId = schedule.Id;
                appointment.PatientId = await GetOrCreatePatientId();
                appointment.Status = "pending";
                appointment.CreatedAt = DateTime.Now;

                _context.Appointments.Add(appointment);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đặt lịch thành công! Vui lòng chờ xác nhận.";
                return RedirectToAction("Index", "Home");
            }

            // Nếu có lỗi validation, load lại danh sách bác sĩ
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

        private async Task<int> GetOrCreatePatientId()
        {
            var patient = await _context.Users.FirstOrDefaultAsync(u => u.Email == "patient@example.com");
            if (patient == null)
            {
                patient = new User
                {
                    Email = "patient@example.com",
                    Password = "123456",
                    FullName = "Bệnh nhân mẫu",
                    Phone = "0123456789",
                    Role = "patient",
                    CreatedAt = DateTime.Now
                };
                _context.Users.Add(patient);
                await _context.SaveChangesAsync();
            }
            return patient.Id;
        }
    }
}