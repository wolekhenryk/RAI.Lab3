using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.WebUtilities;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager,
    IEmailSender emailSender)
    : PageModel
{
    [BindProperty] public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }
    
    public List<SelectListItem> AvailableRoles { get; set; } = new();

    public class InputModel
    {
        [Required, EmailAddress] public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [MinLength(6)]
        public string Password { get; set; } = string.Empty;
        
        [Required, DataType(DataType.Text)]
        [MinLength(2)]
        public string FirstName { get; set; } = string.Empty;
        
        [Required, DataType(DataType.Text)]
        [MinLength(2)]
        public string LastName { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
        public string ConfirmPassword { get; set; } = string.Empty;
        
        [Required, DataType(DataType.Text)]
        public string Role { get; set; } = string.Empty;
    }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        AvailableRoles = AppRoles.AllRoles
            .Select(role => new SelectListItem { Value = role, Text = role })
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            AvailableRoles = AppRoles.AllRoles
                .Select(role => new SelectListItem { Value = role, Text = role })
                .ToList();
            return Page();
        }

        var user = new User
        {
            UserName = Input.Email,
            Email = Input.Email,
            FirstName = Input.FirstName,
            LastName = Input.LastName
        };

        var result = await userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            foreach (var e in result.Errors)
                ModelState.AddModelError(string.Empty, e.Description);
            return Page();
        }

        if (!await roleManager.RoleExistsAsync(Input.Role))
            await roleManager.CreateAsync(new IdentityRole<Guid>(Input.Role));

        if (!await userManager.IsInRoleAsync(user, Input.Role))
            await userManager.AddToRoleAsync(user, Input.Role);

        // Generate email confirmation token
        var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
        code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

        // Create email confirmation callback URL
        var callbackUrl = Url.Page(
            "/Account/ConfirmEmail",
            pageHandler: null,
            values: new { area = "Identity", userId = user.Id, code = code, returnUrl = returnUrl },
            protocol: Request.Scheme);

        // Send verification email
        await emailSender.SendEmailAsync(
            Input.Email,
            "Confirm your email",
            $$"""
              <!DOCTYPE html>
              <html>
              <head>
                  <style>
                      body { font-family: Arial, sans-serif; line-height: 1.6; color: #333; }
                      .container { max-width: 600px; margin: 0 auto; padding: 20px; }
                      .header { background-color: #4CAF50; color: white; padding: 20px; text-align: center; border-radius: 5px 5px 0 0; }
                      .content { padding: 30px; background-color: #f9f9f9; border-radius: 0 0 5px 5px; }
                      .button {
                          display: inline-block;
                          padding: 12px 24px;
                          background-color: #4CAF50;
                          color: white !important;
                          text-decoration: none;
                          border-radius: 4px;
                          margin: 20px 0;
                          font-weight: bold;
                      }
                      .footer { text-align: center; padding: 20px; font-size: 12px; color: #666; margin-top: 20px; }
                  </style>
              </head>
              <body>
                  <div class='container'>
                      <div class='header'>
                          <h1>Welcome to RAI Lab3!</h1>
                      </div>
                      <div class='content'>
                          <h2>Verify Your Email Address</h2>
                          <p>Hello {{HtmlEncoder.Default.Encode(Input.FirstName)}},</p>
                          <p>Thank you for registering with RAI Lab3. Please click the button below to verify your email address and activate your account:</p>
                          <div style='text-align: center;'>
                              <a href='{{HtmlEncoder.Default.Encode(callbackUrl!)}}' class='button'>Verify Email Address</a>
                          </div>
                          <p>Or copy and paste this link into your browser:</p>
                          <p style='word-break: break-all; background-color: #fff; padding: 10px; border: 1px solid #ddd;'>{{HtmlEncoder.Default.Encode(callbackUrl!)}}</p>
                          <p>If you did not create an account, please ignore this email.</p>
                      </div>
                      <div class='footer'>
                          <p>&copy; 2025 RAI Lab3 Application. All rights reserved.</p>
                      </div>
                  </div>
              </body>
              </html>
              """);

        // Redirect to a confirmation page instead of signing in
        return RedirectToPage("RegisterConfirmation", new { email = Input.Email, returnUrl = returnUrl });
    }
}