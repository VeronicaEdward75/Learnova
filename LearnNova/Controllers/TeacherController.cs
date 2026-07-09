using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers;

[Authorize(Roles = "Teacher")]
public class TeacherController : Controller
{
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWalletService _walletService;
    private readonly IWithdrawalService _withdrawalService;
    private readonly IFinanceAnalyticsService _financeAnalyticsService;

    public TeacherController(ICourseService courseService, UserManager<ApplicationUser> userManager, IWalletService walletService, IWithdrawalService withdrawalService, IFinanceAnalyticsService financeAnalyticsService)
    {
        _courseService = courseService;
        _userManager = userManager;
        _walletService = walletService;
        _withdrawalService = withdrawalService;
        _financeAnalyticsService = financeAnalyticsService;
    }

    // ─── AJAX helper ───────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult GetGradesByStage(string stage)
    {
        var grades = CourseFormViewModel.BuildGradeList(stage)
            .Select(item => new { num = item.Value, label = item.Text });
        return Json(grades);
    }

    // ─── Dashboard ─────────────────────────────────────────────────────────────

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة المدرس";
        ViewBag.ActiveNav = "dashboard";

        var teacherId = _userManager.GetUserId(User)!;
        var vm = await _financeAnalyticsService.GetTeacherFinanceDashboardAsync(teacherId);

        return View(vm);
    }

    // ─── Wallet ────────────────────────────────────────────────────────────────

    public async Task<IActionResult> Wallet([Microsoft.AspNetCore.Mvc.FromQuery] LearnNova.Models.ViewModels.Teacher.Filters.TeacherWalletFilterParameters filters)
    {
        ViewData["Title"] = "المحفظة والأرباح";
        ViewBag.ActiveNav = "wallet";

        var teacherId = _userManager.GetUserId(User)!;
        var vm = await _walletService.GetTeacherWalletDashboardAsync(teacherId, filters);

        return View(vm);
    }

    // ─── My Courses ────────────────────────────────────────────────────────────

    public async Task<IActionResult> MyCourses()
    {
        ViewData["Title"] = "كورساتي";
        ViewBag.ActiveNav = "my-courses";

        var teacherId = _userManager.GetUserId(User)!;
        var courses = await _courseService.GetTeacherCoursesAsync(teacherId);
        return View(courses);
    }

    // ─── Create ────────────────────────────────────────────────────────────────

    [HttpGet]
    public IActionResult CreateCourse()
    {
        ViewData["Title"] = "إنشاء كورس جديد";
        ViewBag.ActiveNav = "my-courses";
        return View(new CourseFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCourse(CourseFormViewModel model)
    {
        ViewData["Title"] = "إنشاء كورس جديد";
        ViewBag.ActiveNav = "my-courses";

        // Rebuild grade list so the dropdown re-populates if we return the view
        model.GradeOptions = CourseFormViewModel.BuildGradeList(model.Stage ?? "");

        if (!ModelState.IsValid)
            return View(model);

        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.CreateCourseAsync(model, teacherId);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = $"تم إنشاء الكورس \"{model.Title}\" بنجاح.";
        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Edit ──────────────────────────────────────────────────────────────────

    [HttpGet]
    public async Task<IActionResult> EditCourse(int id)
    {
        ViewData["Title"] = "تعديل الكورس";
        ViewBag.ActiveNav = "my-courses";

        var teacherId = _userManager.GetUserId(User)!;
        var course = await _courseService.GetCourseByIdAsync(id);

        if (course is null || course.TeacherId != teacherId)
        {
            TempData["ErrorMessage"] = "الكورس غير موجود أو غير مصرح لك بتعديله.";
            return RedirectToAction(nameof(MyCourses));
        }

        var model = new CourseFormViewModel
        {
            Title       = course.Title,
            Description = course.Description ?? string.Empty,
            Subject     = course.Subject,
            Stage       = course.Stage,
            GradeLevel  = course.GradeLevel,
            Price       = course.Price,
            GradeOptions = CourseFormViewModel.BuildGradeList(course.Stage)
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditCourse(int id, CourseFormViewModel model)
    {
        ViewData["Title"] = "تعديل الكورس";
        ViewBag.ActiveNav = "my-courses";

        model.GradeOptions = CourseFormViewModel.BuildGradeList(model.Stage ?? "");

        if (!ModelState.IsValid)
            return View(model);

        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.UpdateCourseAsync(id, model, teacherId);

        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error);
            return View(model);
        }

        TempData["SuccessMessage"] = "تم تحديث الكورس بنجاح.";
        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Delete ────────────────────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCourse(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.DeleteCourseAsync(id, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم حذف الكورس بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }

    // ─── Publish / Unpublish ───────────────────────────────────────────────────

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.SetPublishedStatusAsync(id, true, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم نشر الكورس بنجاح." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var result = await _courseService.SetPublishedStatusAsync(id, false, teacherId);

        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم إلغاء نشر الكورس." : string.Join(" ", result.Errors);

        return RedirectToAction(nameof(MyCourses));
    }

    // --- Withdraw -------------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Withdraw()
    {
        ViewData["Title"] = "سحب الأرباح";
        ViewBag.ActiveNav = "wallet";

        var teacherId = _userManager.GetUserId(User)!;
        var wallet = await _walletService.GetWalletAsync(teacherId);
        var withdrawals = await _withdrawalService.GetTeacherWithdrawalsAsync(teacherId);

        var vm = new WithdrawalRequestViewModel
        {
            AvailableBalance = wallet.AvailableBalance,
            MinimumWithdrawal = 100, // From settings normally
            MaximumWithdrawal = 50000,
            HasPendingRequest = withdrawals.Any(w => w.Status == "Pending")
        };

        ViewBag.Withdrawals = withdrawals;

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Withdraw(WithdrawalRequestViewModel model)
    {
        var teacherId = _userManager.GetUserId(User)!;
        
        if (!ModelState.IsValid)
        {
            var wallet = await _walletService.GetWalletAsync(teacherId);
            model.AvailableBalance = wallet.AvailableBalance;
            model.MinimumWithdrawal = 100;
            model.MaximumWithdrawal = 50000;
            ViewBag.Withdrawals = await _withdrawalService.GetTeacherWithdrawalsAsync(teacherId);
            return View(model);
        }

        var result = await _withdrawalService.CreateRequestAsync(teacherId, model);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Wallet));
        }
        else
        {
            TempData["ErrorMessage"] = result.Message;
            return RedirectToAction(nameof(Withdraw));
        }
    }


    // --- Revenue Analytics -----------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Revenue([Microsoft.AspNetCore.Mvc.FromQuery] LearnNova.Models.ViewModels.Teacher.Filters.TeacherRevenueFilterParameters filters)
    {
        ViewData["Title"] = "المالية والأرباح";
        ViewBag.ActiveNav = "revenue";

        var teacherId = _userManager.GetUserId(User)!;
        var vm = await _financeAnalyticsService.GetTeacherRevenuePageAsync(teacherId, filters);
        
        // Populate Course options for the filter dropdown
        var courses = await _courseService.GetTeacherCoursesAsync(teacherId);
        ViewBag.Courses = courses.Select(c => new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem
        {
            Value = c.Id.ToString(),
            Text = c.Title,
            Selected = filters.CourseId == c.Id
        }).ToList();

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportRevenueReport([Microsoft.AspNetCore.Mvc.FromQuery] LearnNova.Models.ViewModels.Teacher.Filters.TeacherRevenueFilterParameters filters)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var vm = await _financeAnalyticsService.GetTeacherRevenuePageAsync(teacherId, filters);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Revenue Report");

        // Headers
        worksheet.Cell(1, 1).Value = "رقم الدفعة";
        worksheet.Cell(1, 2).Value = "اسم الدورة";
        worksheet.Cell(1, 3).Value = "اسم الطالب";
        worksheet.Cell(1, 4).Value = "المبلغ";
        worksheet.Cell(1, 5).Value = "التاريخ";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
        worksheet.SheetView.FreezeRows(1);

        if (!vm.RecentSales.Any())
        {
            worksheet.Cell(2, 1).Value = "لا توجد أرباح مسجلة.";
            worksheet.Range(2, 1, 2, 5).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        }
        else
        {
            int row = 2;
            decimal totalAmount = 0;

            foreach (var sale in vm.RecentSales)
            {
                worksheet.Cell(row, 1).Value = sale.PaymentId;
                worksheet.Cell(row, 2).Value = sale.CourseTitle;
                worksheet.Cell(row, 3).Value = sale.StudentName;
                
                worksheet.Cell(row, 4).Value = sale.Amount;
                worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 EGP";
                
                worksheet.Cell(row, 5).Value = sale.Date;
                worksheet.Cell(row, 5).Style.DateFormat.Format = "yyyy-MM-dd hh:mm AM/PM";

                totalAmount += sale.Amount;
                row++;
            }

            // Totals Row
            worksheet.Cell(row, 1).Value = "الإجمالي";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            
            worksheet.Cell(row, 2).Value = $"العدد: {vm.RecentSales.Count}";
            worksheet.Cell(row, 2).Style.Font.Bold = true;

            worksheet.Cell(row, 4).Value = totalAmount;
            worksheet.Cell(row, 4).Style.Font.Bold = true;
            worksheet.Cell(row, 4).Style.NumberFormat.Format = "#,##0.00 EGP";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        string filename = $"Teacher_Revenue_Report_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    // --- Coupons Management --------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Coupons()
    {
        ViewData["Title"] = "إدارة الكوبونات";
        ViewBag.ActiveNav = "coupons";

        var teacherId = _userManager.GetUserId(User)!;
        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        var coupons = await couponService!.GetTeacherCouponsAsync(teacherId);
        
        var list = new List<LearnNova.Models.ViewModels.Admin.CouponListViewModel>();
        foreach (var c in coupons)
        {
            list.Add(new LearnNova.Models.ViewModels.Admin.CouponListViewModel
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                StartDate = c.StartDate,
                EndDate = c.EndDate,
                UsageLimit = c.UsageLimit,
                IsActive = c.IsActive,
                UsageCount = await couponService.GetCouponUsageCountAsync(c.Id),
                CourseTitle = c.Course?.Title
            });
        }

        return View(list);
    }

    [HttpGet]
    public async Task<IActionResult> CreateCoupon()
    {
        ViewData["Title"] = "إدارة الكوبون";
        ViewBag.ActiveNav = "coupons";
        
        var teacherId = _userManager.GetUserId(User)!;
        var courses = await _courseService.GetTeacherCoursesAsync(teacherId);
        ViewBag.TeacherCourses = courses;

        return View("CouponForm", new LearnNova.Models.ViewModels.Teacher.TeacherCouponFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCoupon(LearnNova.Models.ViewModels.Teacher.TeacherCouponFormViewModel vm)
    {
        ViewData["Title"] = "إدارة الكوبون";
        ViewBag.ActiveNav = "coupons";
        var teacherId = _userManager.GetUserId(User)!;

        if (!ModelState.IsValid) 
        {
            ViewBag.TeacherCourses = await _courseService.GetTeacherCoursesAsync(teacherId);
            return View("CouponForm", vm);
        }

        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        
        var coupon = new Coupon
        {
            Code = vm.Code,
            DiscountType = vm.DiscountType,
            DiscountValue = vm.DiscountValue,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            UsageLimit = vm.UsageLimit,
            CourseId = vm.CourseId,
            TeacherId = teacherId,
            IsActive = vm.IsActive
        };

        var result = await couponService!.CreateCouponAsync(coupon);
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(Coupons));
        }

        ModelState.AddModelError("", result.Message);
        ViewBag.TeacherCourses = await _courseService.GetTeacherCoursesAsync(teacherId);
        return View("CouponForm", vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCouponStatus(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        
        var coupon = await couponService!.GetCouponByIdAsync(id);
        if (coupon == null || coupon.TeacherId != teacherId) return Unauthorized();

        var result = await couponService.ToggleCouponStatusAsync(id);
        
        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;
        
        return RedirectToAction(nameof(Coupons));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var teacherId = _userManager.GetUserId(User)!;
        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;

        var coupon = await couponService!.GetCouponByIdAsync(id);
        if (coupon == null || coupon.TeacherId != teacherId) return Unauthorized();

        var result = await couponService.DeleteCouponAsync(id);
        
        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;
        
        return RedirectToAction(nameof(Coupons));
    }
}
