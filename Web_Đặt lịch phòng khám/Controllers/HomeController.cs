using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using Web_Đặt_lịch_phòng_khám.Models;

namespace Web_Đặt_lịch_phòng_khám.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index() => View();
        public IActionResult Services() => View();
        public IActionResult Contact() => View();
        public IActionResult Privacy() => View();
        public IActionResult Error() => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });

        [HttpPost]
        public IActionResult Consultation(string FullName, string Phone, string Service)
        {
            TempData["Success"] = "Cảm ơn bạn đã đăng ký tư vấn. Chúng tôi sẽ liên hệ sớm!";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult SendContact(string FullName, string Email, string Phone, string Message)
        {
            TempData["Success"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất.";
            return RedirectToAction("Contact");
        }

        // Trang chi tiết dịch vụ
        public IActionResult ServiceDetail(string id)
        {
            var service = GetServiceDetail(id);
            if (service == null) return NotFound();
            ViewBag.Service = service;
            return View();
        }

        private object GetServiceDetail(string id)
        {
            switch (id)
            {
                case "kham-tong-quat":
                    return new
                    {
                        Id = id,
                        Name = "Khám tổng quát",
                        Price = "500.000đ",
                        ShortDesc = "Kiểm tra sức khỏe toàn diện, phát hiện sớm bệnh lý",
                        FullDesc = "Gói khám tổng quát bao gồm: Đo huyết áp, xét nghiệm máu (tổng phân tích, sinh hóa), siêu âm tổng quát (bụng, tim), điện tim, tư vấn dinh dưỡng. Kết quả trong ngày, bác sĩ giàu kinh nghiệm.",
                        Image = "https://benhviennamsaigon.com/wp-content/uploads/2025/03/BS-NB-650x600px.jpg",
                        Features = new List<string> { "Xét nghiệm máu 12 chỉ số", "Siêu âm tổng quát", "Điện tim", "Khám nội khoa chuyên sâu", "Kết quả nhanh trong 2 giờ" },
                        Tests = new List<string> { "Công thức máu", "Đường huyết", "Mỡ máu", "Chức năng gan", "Chức năng thận" },
                        Benefits = new List<string> { "Miễn phí tư vấn dinh dưỡng", "Nhận kết quả qua email", "Hỗ trợ lấy mẫu tại nhà" },
                        Packages = (object)null
                    };
                case "nhi-khoa":
                    return new
                    {
                        Id = id,
                        Name = "Nhi khoa",
                        Price = "400.000đ",
                        ShortDesc = "Chăm sóc sức khỏe trẻ em, tiêm chủng, dinh dưỡng",
                        FullDesc = "Chuyên khoa nhi với bác sĩ giàu kinh nghiệm, phòng khám thân thiện. Dịch vụ: khám phát triển, tư vấn dinh dưỡng, tiêm chủng, điều trị bệnh nhi.",
                        Image = "https://www.thanhcongclinic.com/images/chuyenkhoa/nhi/nhi.jpg",
                        Features = new List<string> { "Khám phát triển toàn diện", "Tiêm chủng theo lịch", "Tư vấn dinh dưỡng cho trẻ", "Điều trị nhi khoa chuyên sâu" },
                        Tests = new List<string> { "Cân nặng, chiều cao", "Xét nghiệm máu", "Xét nghiệm nước tiểu" },
                        Benefits = new List<string> { "Sổ theo dõi sức khỏe miễn phí", "Tư vấn miễn phí qua Zalo", "Ưu đãi tiêm chủng" },
                        Packages =  (object)null
                    };
                case "rang-ham-mat":
                    return new
                    {
                        Id = id,
                        Name = "Răng hàm mặt",
                        Price = "600.000đ",
                        ShortDesc = "Nhổ răng, trám răng, tẩy trắng, niềng răng",
                        FullDesc = "Trung tâm nha khoa với công nghệ CAD/CAM, máy chụp X-quang kỹ thuật số. Dịch vụ: nhổ răng khôn, trám thẩm mỹ, tẩy trắng, niềng răng.",
                        Image = "https://baosonhospital.com/Upload/images-old/images/IMG_7982.JPG",
                        Features = new List<string> { "Nhổ răng khôn an toàn", "Trám răng thẩm mỹ", "Tẩy trắng răng", "Niềng răng các loại" },
                        Tests = new List<string> { "Chụp X-quang", "Khám tổng quát răng miệng" },
                        Benefits = new List<string> { "Bảo hành 6 tháng", "Miễn phí chụp X-quang", "Tặng bàn chải điện" },
                        Packages = (object)null
                    };
                case "tim-mach":
                    return new
                    {
                        Id = id,
                        Name = "Tim mạch",
                        Price = "700.000đ",
                        ShortDesc = "Khám và điều trị bệnh tim mạch, cao huyết áp",
                        FullDesc = "Chuyên khoa tim mạch với máy siêu âm tim 4D, điện tim 12 kênh, theo dõi huyết áp 24h. Bác sĩ chuyên sâu chẩn đoán và điều trị tăng huyết áp, suy tim, rối loạn nhịp.",
                        Image = "https://t-matsuoka.com/wp-content/uploads/2025/06/trong-nhieu-truong-hop-khach-hang-co-the-khong-can-nhin-an-truoc-khi-kham-tim-mach.jpg",
                        Features = new List<string> { "Điện tim", "Siêu âm tim", "Holter huyết áp", "Xét nghiệm men tim" },
                        Tests = new List<string> { "Troponin", "CK-MB", "Lipid máu" },
                        Benefits = new List<string> { "Kết quả nhanh chóng", "Tư vấn lối sống lành mạnh", "Miễn phí đo huyết áp" },
                        Packages = (object)null
                    };
                case "xet-nghiem":
                    return new
                    {
                        Id = id,
                        Name = "Xét nghiệm",
                        Price = "300.000đ - 2.000.000đ",
                        ShortDesc = "Xét nghiệm máu, nước tiểu, chẩn đoán hình ảnh",
                        FullDesc = "Phòng xét nghiệm đạt chuẩn ISO 15189, thiết bị tự động hóa. Các gói xét nghiệm đa dạng, nhận kết quả online nhanh chóng.",
                        Image = "https://images.ctfassets.net/szez98lehkfm/1mtBXXGX9EjiodlR4XtkK6/71edc4efc14285c45c3095a1ee7757d9/MyIC_Inline_75484",
                        Features = new List<string> { "Xét nghiệm máu tổng quát", "Sinh hóa máu (gan, thận, mỡ máu)", "Xét nghiệm miễn dịch (viêm gan, HIV)", "Vi sinh (vi khuẩn, nấm)" },
                        Tests = new List<string> { "Tổng phân tích máu", "Chức năng gan", "Chức năng thận", "Đường huyết" },
                        Benefits = new List<string> { "Kết quả nhanh trong 2-4 giờ", "Lấy mẫu tại nhà (phí 50k)", "Bác sĩ tư vấn miễn phí" },
                        Packages = (object)null
                    };
                case "xet-nghiem-std":
                    return new
                    {
                        Id = id,
                        Name = "Gói xét nghiệm Bệnh xã hội (STDs)",
                        Price = "983.000đ - 1.674.000đ",
                        ShortDesc = "Sàng lọc 15+ bệnh xã hội, phát hiện sớm các tác nhân gây bệnh đường tình dục",
                        FullDesc = "Gói xét nghiệm STD chuyên sâu, phát hiện sớm HIV, giang mai, lậu, herpes, HPV 40 chủng, Chlamydia, vi nấm. Kết quả chính xác, bảo mật tuyệt đối.",
                        Image = "https://medlatec.vn/media/65362/file/xet-nghiem-std-bao-nhieu-tien-3.jpg",
                        Features = new List<string> { "18 chỉ số phát hiện 13+ tác nhân", "HIV Real-time PCR", "HPV Real-time PCR (40 chủng)", "Herpes Simplex Virus (HSV 1&2)", "Chlamydia Real-time PCR", "Vi nấm PCR", "Giang mai (Treponema)" },
                        Tests = new List<string> { "HIV", "HPV 40", "HSV", "Chlamydia", "Vi nấm", "Giang mai" },
                        Benefits = new List<string> { "Bảo mật thông tin tuyệt đối", "Tư vấn kết quả miễn phí", "Gói Cơ bản 17 chỉ số: 983.000đ", "Gói Nâng cao 18 chỉ số: 1.674.000đ" },
                        Packages = new List<object>
                        {
                            new { Name = "Gói Cơ bản", Price = "983.000đ", Features = new List<string> { "17 chỉ số", "Bộ STIs/STDs 13 tác nhân", "Chlamydia PCR", "Vi nấm PCR", "HPV PCR" } },
                            new { Name = "Gói Nâng cao", Price = "1.674.000đ", Features = new List<string> { "18 chỉ số", "Gói cơ bản + HSV Real-time PCR (Herpes 1&2)" } }
                        }
                    };
                default:
                    return null;
            }
        }
    }
}