using Microsoft.EntityFrameworkCore;
using IbukunOluwaPharmaMart.Data;

var builder = WebApplication.CreateBuilder(args);
var port = Environment.GetEnvironmentVariable("PORT") ?? "5001";
builder.WebHost.UseUrls($"http://*:{port}");

builder.Services.AddSession();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllersWithViews();

var conn = builder.Configuration.GetConnectionString("Default");

// Convert Render postgres:// URL to Npgsql format
if (!string.IsNullOrEmpty(conn) && (conn.StartsWith("postgres://") || conn.StartsWith("postgresql://")))
{
    var uri = new Uri(conn);
    var userInfo = uri.UserInfo.Split(':');
    conn = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.Trim('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true;";
}

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
