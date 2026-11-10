using Microsoft.AspNetCore.Authentication.Cookies;
using SkillBridge.DataAccess;
using SkillBridge.DataAccess.Repositories;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
        options.LogoutPath = "/Account/Logout";
        options.AccessDeniedPath = "/Account/Login";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
    });

string connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=localhost;Port=3306;Database=skillbridge_db;Uid=root;Pwd=;";

// Register DataAccess Repositories
builder.Services.AddSingleton(new DBHelper(connectionString));
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<SkillRepository>();
builder.Services.AddScoped<RoadmapRepository>();
builder.Services.AddScoped<ProjectRepository>();
builder.Services.AddScoped<CertificationRepository>();
builder.Services.AddScoped<InternshipRepository>();
builder.Services.AddScoped<ApplicationRepository>();
builder.Services.AddScoped<MentorshipRepository>();
builder.Services.AddScoped<ReadinessRepository>();
builder.Services.AddScoped<ReportRepository>();

var app = builder.Build();

app.UseExceptionHandler(errApp => errApp.Run(async ctx => {
    ctx.Response.StatusCode = 500;
    ctx.Response.ContentType = "text/html";
    var ex = ctx.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
    var msg = ex?.Error?.Message ?? "Unknown error";
    await ctx.Response.WriteAsync($"""<html><body style='font-family:sans-serif;padding:2rem;background:#0f172a;color:#f8fafc'><h2>500 - Server Error</h2><pre style='background:#1e293b;padding:1rem;border-radius:.5rem'>{System.Net.WebUtility.HtmlEncode(msg)}</pre><a href='/' style='color:#6366f1'>← Back to Home</a></body></html>""");
}));
app.UseStaticFiles();

app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
