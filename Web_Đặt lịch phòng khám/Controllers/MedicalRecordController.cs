using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    [Authorize]
    public class MedicalRecordController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public MedicalRecordController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: /MedicalRecord/CreateOrEdit?appointmentId=id
        public async Task<IActionResult> CreateOrEdit(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
                .Include(a => a.User)
                .Include(a => a.Schedule)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
            if (appointment == null) return NotFound();

            // Kiểm tra quyền
            var user = await _userManager.GetUserAsync(User);
            if (User.IsInRole("Doctor"))
            {
                var doctor = await _context.Doctors.FirstOrDefaultAsync(d => d.UserId == user.Id);
                if (doctor == null || appointment.DoctorId != doctor.Id)
                    return Forbid();
            }
            else if (User.IsInRole("Patient"))
            {
                if (appointment.UserId != user.Id)
                    return Forbid();
            }
            else if (!User.IsInRole("Admin"))
            {
                return Forbid();
            }

            var medicalRecord = await _context.MedicalRecords
                .Include(m => m.Prescriptions).ThenInclude(p => p.Medicine)
                .FirstOrDefaultAsync(m => m.AppointmentId == appointmentId);
            if (medicalRecord == null)
            {
                medicalRecord = new MedicalRecord { AppointmentId = appointmentId };
                _context.MedicalRecords.Add(medicalRecord);
                await _context.SaveChangesAsync();
                // Reload to include navigation properties
                medicalRecord = await _context.MedicalRecords
                    .Include(m => m.Prescriptions).ThenInclude(p => p.Medicine)
                    .FirstOrDefaultAsync(m => m.Id == medicalRecord.Id);
            }

            ViewBag.Appointment = appointment;
            ViewBag.Medicines = await _context.Medicines.ToListAsync();

            if (User.IsInRole("Patient"))
            {
                var history = await _context.Appointments
                    .Where(a => a.UserId == appointment.UserId && a.Id != appointmentId && (a.Status == "confirmed" || a.Status == "completed"))
                    .Include(a => a.Doctor).ThenInclude(d => d.Specialty)
                    .Include(a => a.Schedule)
                    .Include(a => a.MedicalRecord).ThenInclude(m => m.Prescriptions).ThenInclude(p => p.Medicine)
                    .OrderByDescending(a => a.Schedule.WorkDate)
                    .ToListAsync();
                ViewBag.MedicalHistory = history;
            }

            return View(medicalRecord);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> UpdateMedicalRecord(int id, string diagnosis, string examinationResult, string notes)
        {
            var medicalRecord = await _context.MedicalRecords.FindAsync(id);
            if (medicalRecord == null) return NotFound();

            medicalRecord.Diagnosis = diagnosis;
            medicalRecord.ExaminationResult = examinationResult;
            medicalRecord.Notes = notes;
            await _context.SaveChangesAsync();

            TempData["Success"] = "Đã cập nhật hồ sơ bệnh án.";
            return RedirectToAction("CreateOrEdit", new { appointmentId = medicalRecord.AppointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> AddPrescription(int medicalRecordId, int medicineId, string dosage, string instructions, int quantity)
        {
            var medicine = await _context.Medicines.FindAsync(medicineId);
            if (medicine == null) return NotFound();

            var prescription = new Prescription
            {
                MedicalRecordId = medicalRecordId,
                MedicineId = medicineId,
                Dosage = dosage,
                Instructions = instructions,
                Quantity = quantity,
                Unit = medicine.Unit,
                Price = medicine.Price
            };
            _context.Prescriptions.Add(prescription);
            await _context.SaveChangesAsync();

            var medicalRecord = await _context.MedicalRecords.FindAsync(medicalRecordId);
            TempData["Success"] = "Đã thêm thuốc vào đơn.";
            return RedirectToAction("CreateOrEdit", new { appointmentId = medicalRecord.AppointmentId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Doctor,Admin")]
        public async Task<IActionResult> DeletePrescription(int id)
        {
            var prescription = await _context.Prescriptions.FindAsync(id);
            if (prescription != null)
            {
                _context.Prescriptions.Remove(prescription);
                await _context.SaveChangesAsync();
            }
            return Redirect(Request.Headers["Referer"].ToString());
        }
    }
}