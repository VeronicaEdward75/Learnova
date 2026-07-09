using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Repositories;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace LearnNova.Controllers;

[Authorize]
public class ProfileController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IWebHostEnvironment _env;
    private readonly IWalletService _walletService;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IGenericRepository<Certificate> _certRepo;
    private readonly LearnNova.Data.ApplicationDbContext _context;

    public ProfileController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IWebHostEnvironment env,
        IWalletService walletService,
        IEnrollmentService enrollmentService,
        IGenericRepository<Certificate> certRepo,
        LearnNova.Data.ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _env = env;
        _walletService = walletService;
        _enrollmentService = enrollmentService;
        _certRepo = certRepo;
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ViewData["Title"] = "الملف الشخصي";
        ViewBag.ActiveNav = "profile";

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var enrollments = (await _enrollmentService.GetStudentCoursesAsync(user.Id)).ToList();
        var certs = await _certRepo.GetQueryable().Where(c => c.StudentId == user.Id).ToListAsync();
        var wallet = await _walletService.GetWalletAsync(user.Id);

        var vm = new ProfileViewModel
        {
            Username = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            StudentId = user.Id,
            FullName = user.FullName,
            PhoneNumber = user.PhoneNumber,
            Gender = user.Gender,
            DateOfBirth = user.DateOfBirth,
            Country = user.Country,
            City = user.City,
            Biography = user.Biography,
            ProfileImagePath = user.ProfileImagePath,
            RegistrationDate = user.CreatedAt,
            LastLoginDate = user.LastLoginDate,
            WalletBalance = wallet?.AvailableBalance ?? 0
        };

        if (User.IsInRole("Admin"))
        {
            vm.TotalUsers = await _context.Users.CountAsync();
            vm.TotalPlatformCourses = await _context.Courses.CountAsync();
            var platformStats = await _walletService.GetPlatformRevenueStatsAsync();
            vm.PlatformRevenue = platformStats.PlatformRevenue;
        }
        else if (User.IsInRole("Teacher"))
        {
            vm.PublishedCourses = await _context.Courses.CountAsync(c => c.TeacherId == user.Id && c.IsPublished);
            vm.TotalStudents = await _context.Enrollments.Include(e => e.Course).Where(e => e.Course.TeacherId == user.Id).Select(e => e.StudentId).Distinct().CountAsync();
            vm.TotalEarnings = wallet?.TotalEarned ?? 0;
        }
        else // Student
        {
            vm.EnrolledCourses = enrollments.Count;
            vm.CompletedCourses = certs.Count; // Assuming 1 cert = 1 completed course
            vm.CertificatesEarned = certs.Count;
        }

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateProfile(ProfileViewModel vm)
    {
        ViewData["Title"] = "الملف الشخصي";
        ViewBag.ActiveNav = "profile";

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        // Repopulate stats in case of validation error
        var enrollments = (await _enrollmentService.GetStudentCoursesAsync(user.Id)).ToList();
        var certs = await _certRepo.GetQueryable().Where(c => c.StudentId == user.Id).ToListAsync();
        var wallet = await _walletService.GetWalletAsync(user.Id);
        
        vm.Username = user.UserName ?? string.Empty;
        vm.Email = user.Email ?? string.Empty;
        vm.StudentId = user.Id;
        vm.RegistrationDate = user.CreatedAt;
        vm.LastLoginDate = user.LastLoginDate;
        vm.WalletBalance = wallet?.AvailableBalance ?? 0;
        vm.ProfileImagePath = user.ProfileImagePath; // Keep current path by default

        if (User.IsInRole("Admin"))
        {
            vm.TotalUsers = await _context.Users.CountAsync();
            vm.TotalPlatformCourses = await _context.Courses.CountAsync();
            var platformStats = await _walletService.GetPlatformRevenueStatsAsync();
            vm.PlatformRevenue = platformStats.PlatformRevenue;
        }
        else if (User.IsInRole("Teacher"))
        {
            vm.PublishedCourses = await _context.Courses.CountAsync(c => c.TeacherId == user.Id && c.IsPublished);
            vm.TotalStudents = await _context.Enrollments.Include(e => e.Course).Where(e => e.Course.TeacherId == user.Id).Select(e => e.StudentId).Distinct().CountAsync();
            vm.TotalEarnings = wallet?.TotalEarned ?? 0;
        }
        else
        {
            vm.EnrolledCourses = enrollments.Count;
            vm.CompletedCourses = certs.Count;
            vm.CertificatesEarned = certs.Count;
        }

        if (!ModelState.IsValid)
        {
            return View("Index", vm);
        }

        // Image Upload
        if (vm.ProfilePicture != null && vm.ProfilePicture.Length > 0)
        {
            if (vm.ProfilePicture.Length > 5 * 1024 * 1024)
            {
                ModelState.AddModelError("ProfilePicture", "حجم الصورة يجب ألا يتجاوز 5 ميجابايت.");
                return View("Index", vm);
            }

            var extension = Path.GetExtension(vm.ProfilePicture.FileName).ToLower();
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            if (!allowedExtensions.Contains(extension))
            {
                ModelState.AddModelError("ProfilePicture", "صيغة غير مدعومة. يسمح فقط بـ jpg, jpeg, png, webp");
                return View("Index", vm);
            }

            var uploadsFolder = Path.Combine(_env.WebRootPath, "uploads", "profiles");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);
            
            var fileName = Guid.NewGuid().ToString() + extension;
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await vm.ProfilePicture.CopyToAsync(fileStream);
            }

            // Delete old picture if exists
            if (!string.IsNullOrEmpty(user.ProfileImagePath))
            {
                var oldPath = Path.Combine(_env.WebRootPath, user.ProfileImagePath.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            user.ProfileImagePath = "/uploads/profiles/" + fileName;
            vm.ProfileImagePath = user.ProfileImagePath;
        }

        // Update fields
        user.FullName = vm.FullName;
        user.PhoneNumber = vm.PhoneNumber;
        user.Gender = vm.Gender;
        user.DateOfBirth = vm.DateOfBirth;
        user.Country = vm.Country;
        user.City = vm.City;
        user.Biography = vm.Biography;

        var result = await _userManager.UpdateAsync(user);
        if (result.Succeeded)
        {
            TempData["SuccessMessage"] = "تم تحديث الملف الشخصي بنجاح.";
            return RedirectToAction(nameof(Index));
        }

        foreach (var error in result.Errors)
        {
            ModelState.AddModelError("", error.Description);
        }

        return View("Index", vm);
    }

    [HttpGet]
    public async Task<IActionResult> Security()
    {
        ViewData["Title"] = "الأمان وكلمة المرور";
        ViewBag.ActiveNav = "security";

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();

        var vm = new SecurityViewModel
        {
            LastLoginDate = user.LastLoginDate
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(SecurityViewModel vm)
    {
        ViewData["Title"] = "الأمان وكلمة المرور";
        ViewBag.ActiveNav = "security";

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return NotFound();
        vm.LastLoginDate = user.LastLoginDate;

        if (!ModelState.IsValid)
        {
            return View("Security", vm);
        }

        var changePasswordResult = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword);
        if (!changePasswordResult.Succeeded)
        {
            foreach (var error in changePasswordResult.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View("Security", vm);
        }

        await _signInManager.RefreshSignInAsync(user);
        TempData["SuccessMessage"] = "تم تغيير كلمة المرور بنجاح.";
        return RedirectToAction(nameof(Security));
    }
}
