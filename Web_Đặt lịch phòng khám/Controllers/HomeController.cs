using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        // Trang chủ hiển thị tất cả chuyên khoa nổi bật
        public async Task<IActionResult> Index()
        {
            var specialties = await _context.Specialties.ToListAsync(); // Đã sửa: lấy tất cả
            return View(specialties);
        }

        // Trang danh sách tất cả chuyên khoa/dịch vụ
        public async Task<IActionResult> Services()
        {
            var services = await _context.Specialties.ToListAsync();
            return View(services);
        }

        public IActionResult Contact() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Consultation(string FullName, string Phone, string Service)
        {
            TempData["Success"] = $"Cảm ơn {FullName} đã đăng ký {Service}. Chúng tôi sẽ gọi đến số {Phone} sớm nhất!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendContact(string FullName, string Email, string Phone, string Message)
        {
            TempData["Success"] = "Tin nhắn của bạn đã được gửi thành công!";
            return RedirectToAction("Contact");
        }

        // Trang chi tiết lấy dữ liệu từ bảng Specialties
        public async Task<IActionResult> ServiceDetail(int id)
        {
            var service = await _context.Specialties
                .Include(s => s.Doctors)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (service == null) return NotFound();

            return View(service);
        }

        public IActionResult Privacy() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}