using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using CeramicsArt.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký MVC
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(
    CookieAuthenticationDefaults.AuthenticationScheme
)
.AddCookie(options =>
{
    options.LoginPath = "/Auth/Login";
    options.LogoutPath = "/Auth/Logout";
    options.AccessDeniedPath = "/Auth/Login";

    options.Cookie.Name = "CeramicsArt.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy =
        CookieSecurePolicy.Always;

    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});
// Kết nối Supabase PostgreSQL

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        npgsqlOptions =>
        {
            // Tăng thời gian chờ truy vấn lên 60 giây
            npgsqlOptions.CommandTimeout(60);

            // Tự động thử lại khi gặp lỗi tạm thời
            npgsqlOptions.EnableRetryOnFailure(
                maxRetryCount: 3,
                maxRetryDelay: TimeSpan.FromSeconds(5),
                errorCodesToAdd: null
            );
        }
    )
);


var app = builder.Build();

// Cấu hình website
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

// Trang chủ
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}"
);

// Đường dẫn kiểm tra database
if (app.Environment.IsDevelopment())
{
    app.MapGet("/kiem-tra-db", async (ApplicationDbContext db) =>
    {
        try
        {
            bool result = await db.Database.CanConnectAsync();

            return result
                ? Results.Ok("CERAMICSART - KET NOI SUPABASE THANH CONG!")
                : Results.Problem("Khong ket noi duoc database.");
        }
        catch (Exception ex)
        {
            return Results.Problem(
                detail: ex.Message,
                statusCode: 500
            );
        }
    });
}

app.Run();
