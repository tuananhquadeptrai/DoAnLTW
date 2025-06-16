using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;

namespace VAYTIEN.Controllers
{
    public class LanguageController : Controller
    {
        // Sửa lại thành HttpGet để hoạt động với thẻ <a>
        [HttpGet]
        public IActionResult SetLanguage(string culture, string returnUrl)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }
    }
}