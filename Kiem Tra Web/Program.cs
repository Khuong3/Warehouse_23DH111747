using Microsoft.EntityFrameworkCore;
using Warehouse_23DH111747.Models;

var builder = WebApplication.CreateBuilder(args);

// Đọc cấu hình thông tin sinh viên từ appsettings.json (nếu có)
var studentSection = builder.Configuration.GetSection("StudentInfo");
if (!string.IsNullOrWhiteSpace(studentSection["FullName"]))
    StudentInfoStore.FullName = studentSection["FullName"]!;
if (!string.IsNullOrWhiteSpace(studentSection["MSSV"]))
    StudentInfoStore.MSSV = studentSection["MSSV"]!;
if (!string.IsNullOrWhiteSpace(studentSection["DateOfBirth"]))
    StudentInfoStore.DateOfBirth = studentSection["DateOfBirth"]!;

// Câu 1: Tự động kết nối SQL Server, tạo CSDL Warehouse_23DH111747 và nạp 6 sản phẩm mẫu
WarehouseDatabaseSeeder.EnsureDatabaseAndSeed();

// Đăng ký Entity Framework DbContext WarehouseEntities
builder.Services.AddDbContext<WarehouseEntities>(options =>
    options.UseSqlServer(WarehouseEntities.DefaultConnectionString));

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Route mặc định trỏ vào trang Quản lý Kho hàng (Products/Index)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Products}/{action=Index}/{id?}");

app.Run();
