using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using SmartGearOnline.Data;
using SmartGearOnline.Models;
using SmartGearOnline.Repositories;
using SmartGearOnline.Services;
using SmartGearOnline.Filters;
using SmartGearOnline.Middleware;
using SmartGearOnline.Hubs;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Npgsql; // <--- added

var builder = WebApplication.CreateBuilder(args);

// add memory cache
builder.Services.AddMemoryCache();

// add SignalR
builder.Services.AddSignalR();

// replace the SQL Server registration with Postgres (Npgsql)
var conn = builder.Configuration.GetConnectionString("DefaultConnection")
           ?? Environment.GetEnvironmentVariable("DefaultConnection")
           ?? throw new InvalidOperationException("DefaultConnection not configured");

builder.Services.AddDbContext<SmartGearContext>(options =>
    options.UseNpgsql(conn));

// Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 6;
    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<SmartGearContext>()
.AddDefaultTokenProviders();

// register repository / services / filters
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<LoggingFilter>();
builder.Services.AddScoped<SimpleAuthorizationFilter>();

builder.Services.AddControllers();

// ----------------------
// Register Repositories & Services
// ----------------------
builder.Services.AddScoped<IProductRepository, ProductRepository>(); // Repository
builder.Services.AddScoped<IProductService, ProductService>();       // Business logic service
builder.Services.AddSingleton<ITimeService, TimeService>();          // Other custom service

// ----------------------
// Register Action Filters (must be before app.Build())
// ----------------------
builder.Services.AddScoped<LoggingFilter>();
builder.Services.AddScoped<SimpleAuthorizationFilter>();

var app = builder.Build();

// run migrations then seed roles (applies on startup; useful when local DNS/outbound blocked)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var db = services.GetRequiredService<SmartGearContext>();
    // apply any pending EF migrations (will run on Render when deployed)
    await db.Database.MigrateAsync();

    await SeedRolesAndAdminAsync(services);
}

app.MapDefaultControllerRoute();
app.MapHub<ProductHub>("/productHub");
app.MapControllers();

// ----------------------
// Built-in middleware
// ----------------------
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();
app.UseRouting();

// ----------------------
// Custom Middleware
// ----------------------
app.UseMiddleware<RequestLoggingMiddleware>();

// add authentication before authorization
app.UseAuthentication();
app.UseAuthorization();

app.MapDefaultControllerRoute();
app.Run();

static async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

    foreach (var role in new[] { "Admin", "User" })
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new IdentityRole(role));

    var adminEmail = "admin@example.com";
    if (await userManager.FindByEmailAsync(adminEmail) is null)
    {
        var admin = new ApplicationUser { UserName = "admin", Email = adminEmail, EmailConfirmed = true };
        await userManager.CreateAsync(admin, "Admin123!");
        await userManager.AddToRoleAsync(admin, "Admin");
    }
}
