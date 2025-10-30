using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RAI.Lab3.Application.Services.Implementation;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Helpers;
using RAI.Lab3.Infrastructure.Repositories.Implementation;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;
using RAI.Lab3.Infrastructure.Security;

var builder = WebApplication.CreateBuilder(args);

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(cs)
        .UseSnakeCaseNamingConvention()
        .UseLazyLoadingProxies();
});

// Repositories

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IRoomRepository, RoomRepository>();
builder.Services.AddScoped<IAvailabilityRepository, AvailabilityRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();

// Services

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IRoomService, RoomService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;
        options.SignIn.RequireConfirmedEmail = false;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build());

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

await app.MigrateAsync<AppDbContext>();
await app.ApplyConstraintsAsync();

app.Run();