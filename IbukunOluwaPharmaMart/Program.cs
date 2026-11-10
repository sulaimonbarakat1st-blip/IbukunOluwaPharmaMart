using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var conn = builder.Configuration.GetConnectionString("Default");

// Auto: SQL Server on your PC, PostgreSQL on Render
if (!string.IsNullOrEmpty(conn) && (conn.Contains("Host=") || conn.Contains("postgres", StringComparison.OrdinalIgnoreCase)))
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseNpgsql(conn));
}
else
{
    builder.Services.AddDbContext<AppDbContext>(o => o.UseSqlServer(conn));
}

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseSession();
app.UseAuthorization();
app.MapStaticAssets();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.Run();