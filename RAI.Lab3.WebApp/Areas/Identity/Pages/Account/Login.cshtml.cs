using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RAI.Lab3.Domain.Models;

namespace RAI.Lab3.WebApp.Areas.Identity.Pages.Account;

[AllowAnonymous]
public class LoginModel : PageModel
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;

    public LoginModel(SignInManager<User> signInManager, UserManager<User> userManager)
    {
        _signInManager = signInManager;
        _userManager = userManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public string? ReturnUrl { get; set; }

    public class InputModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }

    public IActionResult OnGet(string? returnUrl = null)
    {
        // If already signed in, bounce to home (or returnUrl)
        if (User?.Identity?.IsAuthenticated == true)
            return LocalRedirect(returnUrl ?? Url.Content("~/"));

        ReturnUrl = returnUrl;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        returnUrl ??= Url.Content("~/");
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
            return Page();

        // You created users with Email as UserName — sign in by email is fine
        var result = await _signInManager.PasswordSignInAsync(
            userName: Input.Email,
            password: Input.Password,
            isPersistent: Input.RememberMe,
            lockoutOnFailure: true);

        if (result.Succeeded)
            return LocalRedirect(returnUrl);

        if (result.IsLockedOut)
        {
            ModelState.AddModelError(string.Empty, "Account is locked due to multiple failed attempts. Try again later.");
            return Page();
        }

        if (result.RequiresTwoFactor)
        {
            // If you add 2FA later, redirect there
            return RedirectToPage("./LoginWith2fa", new { ReturnUrl = returnUrl, RememberMe = Input.RememberMe });
        }

        ModelState.AddModelError(string.Empty, "Invalid login attempt.");
        return Page();
    }
}
