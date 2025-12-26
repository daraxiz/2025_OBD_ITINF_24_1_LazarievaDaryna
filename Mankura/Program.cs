using Mankura.Data;
using Mankura.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddScoped<Db>();
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<MangaRepository>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ReaderRepository>();
builder.Services.AddScoped<CommentRepository>();
builder.Services.AddScoped<ReadingProcessRepository>();
builder.Services.AddScoped<ChapterRepository>();
builder.Services.AddControllersWithViews();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Account/Login";
    });

builder.Services.AddScoped<ChapterImportService>();

builder.Services.AddAuthorization();
builder.Services.AddSession();

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();

app.UseSession();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();

