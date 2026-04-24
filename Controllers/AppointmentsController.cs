#nullable enable
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;
using Web_Đặt_lịch_phòng_khám.Services;

// Nếu dùng DinkToPdf để xuất hóa đơn (bỏ comment nếu đã cài)
// using DinkToPdf;
// using DinkToPdf.Contracts;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize]
    public class AppointmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailService _emailService;
        // private readonly IConverter _pdfConverter; // Bỏ comment nếu dùng PDF

        public AppointmentsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IEmailService emailService)
        //, IConverter pdfConverter) // thêm nếu dùng PDF
        {
            _context = context;
            _userManager = userManager;
            _emailService = emailService;
            // _pdfConverter = pdfConverter;
        }

        // GET: /Appointments/MyAppointments
        public async Task<IActionResult> MyAppointments()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            IQueryable<Appointment> query = _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d!.Specialty)
                .Include(a => a.Schedule)
                .Include(a => a.User);

            if (User.IsInRole("Admin"))
            {
                ViewBag.IsDoctor = false;
                ViewBag.Title = "Tất cả lịch hẹn";
            }
            else if (User.IsInRole("Doctor"))
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

        // POST: /Appointments/UpdateStatus (Xác nhận/Từ chối)
        [HttpPost]
        [Authorize(Roles = "Doctor")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int appointmentId, string status)
        {
            var appointment = await _context.Appointments
                .Include(a => a.User)
                .Include(a => a.Doctor)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
            if (doctor == null || appointment.DoctorId != doctor.Id)
                return Forbid();

            if (status == "confirmed" || status == "cancelled")
            {
                appointment.Status = status;
                await _context.SaveChangesAsync();
                TempData["Success"] = $"Đã cập nhật trạng thái thành {(status == "confirmed" ? "Đã xác nhận" : "Đã hủy")}.";

                if (status == "confirmed")
                {
                    await _emailService.SendEmailAsync(appointment.User.Email, "Lịch hẹn đã được xác nhận",
                        $@"
                        <h3>Lịch hẹn của bạn đã được xác nhận!</h3>
                        <p>Bác sĩ <strong>{appointment.Doctor?.FullName}</strong> đã xác nhận lịch hẹn vào ngày <strong>{appointment.Schedule?.WorkDate:dd/MM/yyyy}</strong> lúc <strong>{appointment.Schedule?.StartTime}</strong>.</p>
                        <p>Vui lòng đến đúng giờ.</p>
                        ");
                }
                else if (status == "cancelled")
                {
                    await _emailService.SendEmailAsync(appointment.User.Email, "Lịch hẹn bị hủy",
                        $@"
                        <h3>Lịch hẹn của bạn đã bị hủy</h3>
                        <p>Rất tiếc, lịch hẹn với bác sĩ <strong>{appointment.Doctor?.FullName}</strong> vào ngày <strong>{appointment.Schedule?.WorkDate:dd/MM/yyyy}</strong> lúc <strong>{appointment.Schedule?.StartTime}</strong> đã bị hủy. Vui lòng đặt lịch lại.</p>
                        ");
                }
            }
            else
            {
                TempData["Error"] = "Trạng thái không hợp lệ.";
            }
            return RedirectToAction(nameof(MyAppointments));
        }

        // POST: /Appointments/Delete
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var appointment = await _context.Appointments.FindAsync(id);
            if (appointment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var isOwner = appointment.UserId == user.Id;
            var isDoctor = false;
            var isAdmin = User.IsInRole("Admin");
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor != null && appointment.DoctorId == doctor.Id)
                    isDoctor = true;
            }

            if (!isOwner && !isDoctor && !isAdmin)
                return Forbid();

            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã xóa lịch hẹn thành công.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // POST: /Appointments/UpdatePaymentStatus (cập nhật thanh toán)
        [HttpPost]
        [Authorize(Roles = "Doctor,Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int appointmentId)
        {
            var appointment = await _context.Appointments.FindAsync(appointmentId);
            if (appointment == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();
            }

            if (appointment.PaymentStatus == "Paid")
            {
                TempData["Error"] = "Lịch hẹn này đã được thanh toán.";
                return RedirectToAction(nameof(MyAppointments));
            }

            appointment.PaymentStatus = "Paid";
            await _context.SaveChangesAsync();
            TempData["Success"] = "Đã cập nhật trạng thái thanh toán thành Đã thanh toán.";
            return RedirectToAction(nameof(MyAppointments));
        }

        // GET: /Appointments/ExportInvoice/{id}
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> ExportInvoice(int id)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (appointment == null) return NotFound();

            // Nếu chưa thanh toán thì không cho xuất hóa đơn
            if (appointment.PaymentStatus != "Paid")
                return BadRequest("Lịch hẹn chưa được thanh toán, không thể xuất hóa đơn.");

            // Tạo HTML cho hóa đơn
            var htmlContent = $@"
            <html>
            <head><meta charset='utf-8'/><title>Hóa đơn khám bệnh</title></head>
            <body style='font-family:Arial; padding:20px'>
                <h2 style='text-align:center'>PHÒNG KHÁM ĐẶT LỊCH</h2>
                <h3 style='text-align:center'>HÓA ĐƠN KHÁM BỆNH</h3>
                <hr />
                <p><strong>Mã hóa đơn:</strong> INV-{appointment.Id:D6}</p>
                <p><strong>Ngày lập:</strong> {DateTime.Now:dd/MM/yyyy HH:mm}</p>
                <p><strong>Bệnh nhân:</strong> {appointment.User?.FullName} ({appointment.User?.Email})</p>
                <p><strong>Bác sĩ:</strong> {appointment.Doctor?.FullName}</p>
                <p><strong>Chuyên khoa:</strong> {appointment.Doctor?.Specialty?.Name}</p>
                <p><strong>Ngày khám:</strong> {appointment.Schedule?.WorkDate:dd/MM/yyyy}</p>
                <p><strong>Giờ khám:</strong> {appointment.Schedule?.StartTime} - {appointment.Schedule?.EndTime}</p>
                <p><strong>Phí khám:</strong> {(appointment.ConsultationFee ?? 0):N0} VNĐ</p>
                <p><strong>Trạng thái thanh toán:</strong> Đã thanh toán</p>
                <hr />
                <p style='text-align:center'>Cảm ơn quý khách đã sử dụng dịch vụ!</p>
            </body>
            </html>";

            // Sử dụng DinkToPdf để xuất PDF (nếu đã cài)
            // var pdfDocument = new HtmlToPdfDocument()
            // {
            //     GlobalSettings = { ColorMode = ColorMode.Color, Orientation = Orientation.Portrait, PaperSize = PaperKind.A4 },
            //     Objects = { new ObjectSettings { HtmlContent = htmlContent, WebSettings = { DefaultEncoding = "utf-8" } } }
            // };
            // var pdfBytes = _pdfConverter.Convert(pdfDocument);
            // return File(pdfBytes, "application/pdf", $"Invoice_{appointment.Id}.pdf");

            // Tạm thời trả về HTML (nếu chưa cài PDF)
            return Content(htmlContent, "text/html");
        }

        // GET: /Appointments/Create
        [HttpGet]
        [Authorize(Roles = "Patient")]
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
        [Authorize(Roles = "Patient")]
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

            var existingAppointment = await _context.Appointments.FirstOrDefaultAsync(a => a.ScheduleId == schedule.Id);
            if (existingAppointment != null)
            {
                ModelState.AddModelError("", "Khung giờ này đã có người đặt. Vui lòng chọn giờ khác.");
                await LoadDoctorsToViewBag();
                return View();
            }

            var doctor = await _context.Doctors.FindAsync(doctorId);
            var appointment = new Appointment
            {
                UserId = user.Id,
                DoctorId = doctorId,
                ScheduleId = schedule.Id,
                Symptoms = symptoms ?? string.Empty,
                Status = "pending",
                CreatedAt = DateTime.Now,
                ConsultationFee = doctor?.ConsultationFee ?? 0,
                PaymentStatus = "Pending"  // Chưa thanh toán khi mới đặt
            };

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync();

            var doctorName = doctor?.FullName ?? "Bác sĩ";
            await _emailService.SendEmailAsync(user.Email, "Xác nhận đặt lịch",
                $@"
                <h3>Cảm ơn bạn đã đặt lịch!</h3>
                <p>Bạn đã đặt lịch với bác sĩ <strong>{doctorName}</strong> vào ngày <strong>{workDate:dd/MM/yyyy}</strong> lúc <strong>{appointmentTime}</strong>.</p>
                <p>Vui lòng chờ bác sĩ xác nhận. Bạn có thể theo dõi trạng thái trong mục <a href='/Appointments/MyAppointments'>Lịch hẹn của tôi</a>.</p>
                <p>Trân trọng,<br/>Phòng khám Đặt lịch</p>
                ");

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