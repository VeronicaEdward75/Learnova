using System.Threading.Tasks;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;
using LearnNova.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Collections.Generic;

namespace LearnNova.Controllers;

public class CertificateController : Controller
{
    private readonly ICertificateService _certificateService;
    private readonly UserManager<ApplicationUser> _userManager;

    public CertificateController(ICertificateService certificateService, UserManager<ApplicationUser> userManager)
    {
        _certificateService = certificateService;
        _userManager = userManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Verify(string? code)
    {
        ViewData["Title"] = "التحقق من الشهادة";
        ViewBag.Code = code;
        return View();
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> VerifyPost(string code)
    {
        ViewData["Title"] = "التحقق من الشهادة";
        ViewBag.Code = code;

        if (string.IsNullOrWhiteSpace(code))
        {
            ViewBag.Error = "يرجى إدخال كود التحقق أو رقم الشهادة.";
            return View("Verify");
        }

        var cert = await _certificateService.GetCertificateByCodeAsync(code);
        if (cert == null)
        {
            ViewBag.IsValid = false;
        }
        else
        {
            ViewBag.IsValid = true;
            ViewBag.Certificate = cert;
        }

        return View("Verify");
    }

    [HttpGet]
    [Authorize(Roles = "Student")]
    public async Task<IActionResult> MyCertificates([FromServices] IGenericRepository<Enrollment> enrollmentRepo, [FromQuery] LearnNova.Models.ViewModels.Student.Filters.CertificateFilterParameters filters)
    {
        ViewData["Title"] = "شهاداتي";
        ViewBag.ActiveNav = "certificates";
        var studentId = _userManager.GetUserId(User)!;

        // 1. Fetch all enrollments for missing requirements check
        var enrollments = await enrollmentRepo.GetQueryable()
            .Include(e => e.Course)
            .Where(e => e.StudentId == studentId && e.IsActive)
            .ToListAsync();

        var missingRequirements = new Dictionary<string, string>();

        foreach (var enrollment in enrollments)
        {
            var res = await _certificateService.CheckAndIssueCertificateAsync(studentId, enrollment.CourseId);
            if (!res.Success)
            {
                missingRequirements[enrollment.Course.Title] = res.Message;
            }
        }

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("اسم الكورس (أ-ي)", "name_asc"),
            new("اسم الكورس (ي-أ)", "name_desc")
        };

        var certs = await _certificateService.GetStudentCertificatesPagedAsync(studentId, filters);
        
        ViewBag.MissingRequirements = missingRequirements;
        ViewBag.Filters = filters;

        return View(certs);
    }
}
