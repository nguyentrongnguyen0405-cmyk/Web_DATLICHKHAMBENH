#nullable enable
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Web_Đặt_lịch_phòng_khám.Data;
using Web_Đặt_lịch_phòng_khám.Models;
using Web_Đặt_lịch_phòng_khám.Services;
using Web_Đặt_lịch_phòng_khám.Repositories;
using DinkToPdf;
using DinkToPdf.Contracts;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Cấu hình Identity với chính sách mật khẩu linh hoạt (chấp nhận mật khẩu vừa đủ mạnh)
builder.Services.AddDefaultIdentity<ApplicationUser>(options => {
    options.Password.RequireDigit = true;       // yêu cầu có số
    options.Password.RequiredLength = 6;        // độ dài tối thiểu 6
    options.Password.RequireNonAlphanumeric = true; // yêu cầu ký tự đặc biệt ( @, #, $, ... )
    options.Password.RequireUppercase = true;   // yêu cầu chữ hoa
    options.Password.RequireLowercase = true;   // yêu cầu chữ thường
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// Đăng ký các service
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<IQRCodeService, QRCodeService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddSingleton(typeof(IConverter), new SynchronizedConverter(new PdfTools()));

builder.Services.AddControllersWithViews();
builder.Services.AddRazorPages();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

 //Tạm thời comment DbInitializer để tránh lỗi trong quá trình test
using (var scope = app.Services.CreateScope())
 {
     var services = scope.ServiceProvider;
     try
     {
        await DbInitializer.InitializeAsync(services);
         Console.WriteLine("DbInitializer chạy thành công.");
     }
    catch (Exception ex)
     {
         Console.WriteLine($"Lỗi khi chạy DbInitializer: {ex.Message}");
     }
 }

app.Run();