using FruitSA_Data.Context;
using FruitSA_DataAccess.BusinessLogic;
using FruitSA_DataAccess.BusinessLogic.IBusinessLogic;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using OfficeOpenXml;
using FruitSA_Assessment.DataAccess.DbInitializer;

var builder = WebApplication.CreateBuilder(args);

// Set EPPlus license context
ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(
  builder.Configuration.GetConnectionString("DefaultConnection")
  ));

builder.Services.AddIdentity<IdentityUser, IdentityRole>().AddDefaultTokenProviders()
  .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

builder.Services.AddScoped<ICategory_Business, Category_Business>();
builder.Services.AddScoped<IProduct_Business, Product_Business>();
builder.Services.AddScoped<IDbInitializer, DbInitializer>();


builder.Services.AddRazorPages().AddRazorRuntimeCompilation();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = $"/Identity/Account/Login";
    options.LogoutPath = $"/Identity/Account/Logout";
    options.AccessDeniedPath = $"/Identity/Account/AccessDenied";
});
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(100);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
SeedDatabase();
app.UseAuthentication();

app.UseAuthorization();

app.UseSession();
app.MapRazorPages();

app.MapControllerRoute(
  name: "default",
  pattern: "{area=Customer}/{controller=Home}/{action=Index}/{id?}");

app.Run();

void SeedDatabase()
{
    using (var scope = app.Services.CreateScope())
    {
        var dbInitializer = scope.ServiceProvider.GetRequiredService<IDbInitializer>();
        dbInitializer.Initialize();
    }
}