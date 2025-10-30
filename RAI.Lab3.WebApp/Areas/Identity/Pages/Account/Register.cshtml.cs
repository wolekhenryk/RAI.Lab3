using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using RAI.Lab3.Domain.Models;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class RegisterModel(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<IdentityRole<Guid>> roleManager)
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

        await signInManager.SignInAsync(user, isPersistent: false);
        return LocalRedirect(returnUrl);
    }
}