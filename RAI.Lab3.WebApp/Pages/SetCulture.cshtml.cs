using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace RAI.Lab3.WebApp.Pages;

public class SetCultureModel : PageModel
{
    public IActionResult OnGet(string culture, string returnUrl = "/")
    {
        if (string.IsNullOrEmpty(culture))
        {
            return LocalRedirect(returnUrl);
        }

        Response.Cookies.Append(
            CookieRequestCultureProvider.DefaultCookieName,
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
            new CookieOptions
            {
                Expires = DateTimeOffset.UtcNow.AddYears(1),
                IsEssential = true,
                SameSite = SameSiteMode.Lax
            }
        );

        return LocalRedirect(returnUrl);
    }
}
