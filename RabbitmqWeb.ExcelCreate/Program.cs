using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RabbitmqWeb.ExcelCreate.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer"));
});

builder.Services.AddIdentity<IdentityUser, IdentityRole>(opt =>
{
    opt.User.RequireUniqueEmail = true;
    
}).AddEntityFrameworkStores<AppDbContext>();

var app = builder.Build();

var appDbContext = app.Services.GetRequiredService<AppDbContext>();
var userManager = app.Services.GetRequiredService<UserManager<IdentityUser>>();
appDbContext.Database.Migrate();

if (!appDbContext.Users.Any())
{
    userManager.CreateAsync(new IdentityUser()
    {
        UserName = "demirdogukan",
        Email = "demir@test.com"
    }, "Password12").Wait();
    
    userManager.CreateAsync(new IdentityUser()
    {
        UserName = "test1",
        Email = "test1@test.com"
    }, "Password12").Wait();
}

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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();