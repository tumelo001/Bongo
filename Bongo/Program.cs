using Bongo.Data;
using Bongo.Infrastructure;
using Bongo.Models;
using Bongo.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<IRepositoryWrapper, RepositoryWrapper>();
builder.Services.AddTransient<IMailService, MailService>();
/*builder.Services.AddScoped<IHttpContextAccessor , HttpContextAccessor>(); 
*/
builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services.AddIdentity<BongoUser, IdentityRole>( options =>
{
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 5;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true; 
}).AddEntityFrameworkStores<AppDbContext>().AddTokenProvider<DataProtectorTokenProvider<BongoUser>>(TokenOptions.DefaultProvider);

builder.Services.AddScoped<IPasswordValidator<BongoUser>, CustomPasswordValidator>();

var app = builder.Build();


if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();    
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=LandingPage}/{action=Index}/{id?}/{string?}");

SeedData.EnsurePopulated(app);
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense(app.Configuration.GetValue<string>("SyncfusionKey:Key"));

app.Run();
