using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.EntityFrameworkCore;
using VAYTIEN.Models;
using Microsoft.Extensions.DependencyInjection;
using VAYTIEN.Services;
var builder = WebApplication.CreateBuilder(args);

// Cấu hình DbContext và Identity
builder.Services.AddDbContext<QlvayTienContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>() // Thêm hỗ trợ role
.AddEntityFrameworkStores<QlvayTienContext>();

// Đăng ký RoleManager vào DI container
builder.Services.AddScoped<RoleManager<IdentityRole>>();
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "VayTienSession"; // tên cookie
    options.ExpireTimeSpan = TimeSpan.FromHours(1); // hết hạn sau 1 giờ
    options.SlidingExpiration = false; // không kéo dài khi sử dụng
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<PdfGenerator>();
builder.Services.AddScoped<EmailSender>();
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseForceLogoutOnStartup();
app.UseAuthentication();
app.UseAuthorization();

// Route cho Area
app.MapAreaControllerRoute(
    name: "Admin",
    areaName: "Admin",
    pattern: "Admin/{controller=Dashboard}/{action=Index}/{id?}");

// Route mặc định
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages();


using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    string[] roles = { SD.Role_Admin, SD.Role_Customer, SD.Role_Company, SD.Role_Employee };

    foreach (var role in roles)
    {
        if (!await roleManager.RoleExistsAsync(role))
        {
            await roleManager.CreateAsync(new IdentityRole(role));
        }
    }
}

app.Run();
