using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using VAYTIEN.Models;

namespace VAYTIEN.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public UserController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleLock(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            if (user.LockoutEnd != null && user.LockoutEnd > DateTime.Now)
            {
                user.LockoutEnd = null; // mở khóa
            }
            else
            {
                user.LockoutEnd = DateTime.Now.AddYears(100); // khóa vĩnh viễn
            }

            await _userManager.UpdateAsync(user);
            return RedirectToAction("Index");
        }
    }
}
