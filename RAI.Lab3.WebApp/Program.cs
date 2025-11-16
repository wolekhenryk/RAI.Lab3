using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using QuestPDF.Infrastructure;
using RAI.Lab3.Application.Dto;
using RAI.Lab3.Application.Services.Implementation;
using RAI.Lab3.Application.Services.Interfaces;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Data;
using RAI.Lab3.Infrastructure.Helpers;
using RAI.Lab3.Infrastructure.Repositories.Implementation;
using RAI.Lab3.Infrastructure.Repositories.Interfaces;
using RAI.Lab3.Infrastructure.Security;
using RAI.Lab3.WebApp.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

var cs = builder.Configuration.GetConnectionString("DefaultConnection")
         ?? throw new InvalidOperationException("Missing DefaultConnection");

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options
        .UseNpgsql(cs)
        .UseSnakeCaseNamingConvention()
        .UseLazyLoadingProxies()
        .EnableSensitiveDataLogging();
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
builder.Services.AddScoped<IReservationService, ReservationService>();
builder.Services.AddTransient<IEmailSender, SmtpEmailSender>();

builder.Services.AddDefaultIdentity<User>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<AppDbContext>()
    .AddDefaultUI();

// JWT Authentication
const string jwtKey = "my-super-secret-key-minimum-32-chars-requiredmy-super-secret-key-minimum-32-chars-requiredmy-super-secret-key-minimum-32-chars-required!";
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

builder.Services.AddAuthentication()
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(new AuthorizationPolicyBuilder(
        IdentityConstants.ApplicationScheme,
        JwtBearerDefaults.AuthenticationScheme)
        .RequireAuthenticatedUser()
        .Build());


// Localization
var supportedCultures = new[]
{
    new CultureInfo("pl-PL"),
    new CultureInfo("en-US")
};

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new RequestCulture("pl-PL");
    options.SupportedCultures = supportedCultures;
    options.SupportedUICultures = supportedCultures;
    options.RequestCultureProviders = new List<IRequestCultureProvider>
    {
        new CookieRequestCultureProvider()
    };
});

builder.Services.AddRazorPages(options =>
{
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Login");
    options.Conventions.AllowAnonymousToAreaPage("Identity", "/Account/Register");
    options.Conventions.AllowAnonymousToPage("/SetCulture");
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Identity/Account/Login";
    options.AccessDeniedPath = "/Identity/Account/AccessDenied";

    // For API requests, return 401 instead of redirecting to login
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };

    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }
        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRequestLocalization();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// API Endpoints
app.MapGet("/api/slots/available", async (IAvailabilityService availabilityService) =>
{
    var availableSlotsResult = await availabilityService.GetAllAvailabilitiesAsync();
    return availableSlotsResult.IsSuccess ?
        Results.Ok(availableSlotsResult.Value) :
        Results.Problem(availableSlotsResult.Error.Message);
}).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

app.MapGet("/api/rooms", async (IRoomService roomService) =>
{
    var rooms = await roomService.GetAllRoomsAsync();
    return rooms.IsSuccess ?
        Results.Ok(rooms.Value) :
        Results.Problem(rooms.Error.Message);
}).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

app.MapPost("api/slots/{id:guid}/book", async (Guid id, IReservationService reservationService) =>
{
    var reservationResult = await reservationService.CreateReservationAsync(id);
    return reservationResult.IsSuccess ?
        Results.Ok() :
        Results.Problem(reservationResult.Error.Message);
}).RequireAuthorization(new AuthorizeAttribute { AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme });

QuestPDF.Settings.License = LicenseType.Community;

app.MapPost("api/illegal-login", async (
    IllegalUserLoginDto illegalUserLoginDto,
    UserManager<User> userManager) =>
{
    var user = await userManager.FindByEmailAsync(illegalUserLoginDto.Email);
    if (user is null)
        return Results.Unauthorized();

    var passwordValid = await userManager.CheckPasswordAsync(user, illegalUserLoginDto.Password);
    if (!passwordValid)
        return Results.Unauthorized();

    //var jwtKey = "my-super-secret-key-minimum-32-chars-required!";
    //var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

    // Get user roles
    var roles = await userManager.GetRolesAsync(user);

    // Build claims list
    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Email, user.Email!),
        new(ClaimTypes.Name, user.UserName!)
    };

    // Add role claims
    foreach (var role in roles)
    {
        claims.Add(new Claim(ClaimTypes.Role, role));
    }

    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
    );

    return Results.Ok(new {
        token = new JwtSecurityTokenHandler().WriteToken(token)
    });
}).AllowAnonymous();

app.MapRazorPages();

await app.MigrateAsync<AppDbContext>();
await app.ApplyConstraintsAsync();

app.UseSwagger();
app.UseSwaggerUI();

app.Run();