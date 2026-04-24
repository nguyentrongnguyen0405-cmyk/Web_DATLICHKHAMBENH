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
        // Hiển thị chi tiết nhóm xét nghiệm
        // Hiển thị chi tiết nhóm xét nghiệm
        public IActionResult GroupDetail(string group)
        {
            ViewBag.Group = group;

            // Dữ liệu chi tiết cho từng nhóm (có thể lấy từ database sau)
            var groupData = new Dictionary<string, dynamic>
            {
                ["Nữ giới"] = new
                {
                    Title = "Gói xét nghiệm dành cho Nữ giới",
                    Description = "Các gói xét nghiệm chuyên sâu giúp phụ nữ chủ động chăm sóc sức khỏe sinh sản, phát hiện sớm các bệnh lý phụ khoa, ung thư cổ tử cung, ung thư vú.",
                    Tests = new List<string> { "Xét nghiệm HPV", "Xét nghiệm tế bào cổ tử cung (Pap smear)", "Siêu âm vú", "Xét nghiệm nội tiết tố nữ (Estrogen, Progesterone)" },
                    Price = "Liên hệ",
                    Image = "https://cdn1.concung.com/storage/data/2021/sweet-baby-gco/gioi-thieu-den-me-top-3-phong-kham-san-phu-khoa-lam-dong-uy-tin/cac-dich-vu-tai-phong-kham-san-phu-khoa-tai-lam-dong-nguon-internet.webp"
                },
                ["Nam giới"] = new
                {
                    Title = "Gói xét nghiệm dành cho Nam giới",
                    Description = "Kiểm tra sức khỏe nam khoa toàn diện, đánh giá chức năng sinh sản, tầm soát ung thư tuyến tiền liệt, bệnh lý nội tiết.",
                    Tests = new List<string> { "Xét nghiệm testosterone", "Tinh dịch đồ", "PSA (ung thư tuyến tiền liệt)", "Siêu âm tinh hoàn" },
                    Price = "Liên hệ",
                    Image = "https://medlatec.vn/media/30673/file/_nam-khoa-2.jpg"
                },
                ["Mẹ bầu"] = new
                {
                    Title = "Gói xét nghiệm dành cho Mẹ bầu",
                    Description = "Theo dõi sức khỏe thai kỳ, sàng lọc dị tật thai nhi, kiểm tra các bệnh lý nguy cơ trong thai kỳ.",
                    Tests = new List<string> { "Xét nghiệm Double test, Triple test", "Tiểu đường thai kỳ", "Viêm gan B, C, giang mai, HIV", "Siêu âm hình thái học" },
                    Price = "Liên hệ",
                    Image = "https://medlatec.vn/media/50441/file/kham-thai-tai-go-vap-1.jpg"
                },
                ["Tiền hôn nhân"] = new
                {
                    Title = "Gói xét nghiệm Tiền hôn nhân",
                    Description = "Kiểm tra sức khỏe tổng thể trước khi kết hôn, phát hiện các bệnh lý di truyền, bệnh truyền nhiễm ảnh hưởng đến hôn nhân và sinh sản.",
                    Tests = new List<string> { "Xét nghiệm máu tổng quát", "Viêm gan B, C, HIV, giang mai", "Nhóm máu, yếu tố Rh", "Xét nghiệm tinh dịch đồ (nam)" },
                    Price = "Liên hệ",
                    Image = "https://benhviendongnai.com.vn/wp-content/uploads/2025/01/kham-suc-khoe-tien-hon-nhan-2.jpg"
                },
                ["Trẻ em"] = new
                {
                    Title = "Gói xét nghiệm dành cho Trẻ em",
                    Description = "Đánh giá sự phát triển thể chất, tầm soát các bệnh nhi khoa thường gặp, dinh dưỡng và miễn dịch.",
                    Tests = new List<string> { "Công thức máu", "Xét nghiệm vi chất (sắt, kẽm, vitamin D)", "Xét nghiệm dị ứng", "Xét nghiệm viêm gan, giun sán" },
                    Price = "Liên hệ",
                    Image = "https://datkhamnhanh.vn/upload/2025/12/18/mceu_46183833611766072095098.jpg"
                },
                ["Người cao tuổi"] = new
                {
                    Title = "Gói xét nghiệm dành cho Người cao tuổi",
                    Description = "Tầm soát các bệnh lý thường gặp ở người già: tim mạch, tiểu đường, loãng xương, suy giảm chức năng gan thận.",
                    Tests = new List<string> { "Công thức máu, đường huyết, mỡ máu", "Chức năng gan, thận", "Điện tim", "Xét nghiệm loãng xương" },
                    Price = "Liên hệ",
                    Image = "https://bacsigiadinhhanoi.vn/wp-content/uploads/2021/09/kham-benh-nguoi-gia.jpg"
                }
            };

            if (groupData.ContainsKey(group))
            {
                ViewBag.Data = groupData[group];
            }
            else
            {
                ViewBag.Data = null;
            }

            return View();
        }
    }

}