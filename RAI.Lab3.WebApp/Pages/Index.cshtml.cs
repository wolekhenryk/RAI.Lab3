using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using RAI.Lab3.Infrastructure.Roles;

namespace RAI.Lab3.WebApp.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        // Redirect students to the SignUpForProject page by default
        if (User.IsInRole(AppRoles.Student))
        {
            return RedirectToPage("/SignUpForProject");
        }

        // Redirect teachers to the DeclareAvailability page by default
        if (User.IsInRole(AppRoles.Teacher))
        {
            return RedirectToPage("/DeclareAvailability");
        }

        return Page();
    }
}