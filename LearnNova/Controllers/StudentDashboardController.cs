using System.Threading.Tasks;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers
{
    [Authorize(Roles = "Student")]
    public class StudentDashboardController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IStudentDashboardService _studentDashboardService;

        public StudentDashboardController(
            UserManager<ApplicationUser> userManager,
            IStudentDashboardService studentDashboardService)
        {
            _userManager = userManager;
            _studentDashboardService = studentDashboardService;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            ViewData["Title"] = "لوحة التحكم";
            ViewBag.ActiveNav = "dashboard";

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var vm = await _studentDashboardService.GetDashboardAsync(
                user.Id,
                user.FullName ?? user.UserName ?? "طالب");

            return View(vm);
        }
    }
}
