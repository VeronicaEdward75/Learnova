using LearnNova.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace LearnNova.Controllers
{
    // Trivial authenticated landing page until teammates build the real Teacher/Student
    // dashboards (see Document/04-Next-Steps-Plan.md §8) — this is not our part to build out.
    [AllowAnonymous]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                if (User.IsInRole("Admin"))
                    return RedirectToAction("Dashboard", "Admin");

                if (User.IsInRole("Teacher"))
                    return RedirectToAction("Dashboard", "Teacher");

                if (User.IsInRole("Student"))
                    return RedirectToAction("Index", "StudentDashboard");
            }

            return View("LandingPage");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(int? statusCode = null)
        {
            var exceptionHandlerPathFeature = HttpContext.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerPathFeature>();
            if (exceptionHandlerPathFeature?.Error != null)
            {
                _logger.LogError(exceptionHandlerPathFeature.Error, "Unhandled exception occurred while processing request {Path}", exceptionHandlerPathFeature.Path);
            }

            var viewName = statusCode switch
            {
                404 => "Error404",
                403 => "Error403",
                _ => "Error500"
            };

            Response.StatusCode = statusCode ?? 500;
            return View(viewName, new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
