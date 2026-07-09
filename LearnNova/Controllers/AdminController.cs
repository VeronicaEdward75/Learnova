using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels;
using LearnNova.Models.ViewModels.Admin;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
namespace LearnNova.Controllers;

[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
    private readonly IUserService _userService;
    private readonly ICourseService _courseService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWalletService _walletService;
    private readonly IWithdrawalService _withdrawalService;
    private readonly IFinanceAnalyticsService _financeAnalyticsService;

    public AdminController(IUserService userService, ICourseService courseService, UserManager<ApplicationUser> userManager, IWalletService walletService, IWithdrawalService withdrawalService, IFinanceAnalyticsService financeAnalyticsService)
    {
        _userService = userService;
        _courseService = courseService;
        _userManager = userManager;
        _walletService = walletService;
        _withdrawalService = withdrawalService;
        _financeAnalyticsService = financeAnalyticsService;
    }

    public async Task<IActionResult> Dashboard()
    {
        ViewData["Title"] = "لوحة التحكم";
        ViewBag.ActiveNav = "dashboard";

        var courses = (await _courseService.GetAllCoursesAsync()).ToList();
        var stats = await _walletService.GetPlatformRevenueStatsAsync();

        var vm = new AdminDashboardViewModel
        {
            TotalStudents = (await _userService.GetUsersByRoleAsync(UserRole.Student)).Count(),
            TotalTeachers = (await _userService.GetUsersByRoleAsync(UserRole.Teacher)).Count(),
            TotalAdmins = (await _userService.GetUsersByRoleAsync(UserRole.Admin)).Count(),
            PendingTeachersCount = (await _userService.SearchUsersAsync(null, UserRole.Teacher, false)).Count(),
            TotalCourses = courses.Count,
            PublishedCoursesCount = courses.Count(c => c.IsPublished),
            TotalPlatformRevenue = stats.PlatformRevenue,
            TotalTeacherEarnings = stats.TeacherEarnings,
            PendingRevenue = stats.PendingRevenue
        };

        return View(vm);
    }

    public async Task<IActionResult> Users(string? term, UserRole? role, bool? isActive)
    {
        ViewData["Title"] = "المستخدمون";
        ViewBag.ActiveNav = "users";
        ViewBag.Term = term ?? "";
        ViewBag.RoleFilter = role?.ToString() ?? "";
        ViewBag.IsActiveFilter = isActive is null ? "" : isActive.Value ? "true" : "false";
        ViewBag.CurrentUserId = _userManager.GetUserId(User);

        var users = await _userService.SearchUsersAsync(term, role, isActive);
        return View(users);
    }

    public async Task<IActionResult> UserDetails(string id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        if (user is null)
        {
            TempData["ErrorMessage"] = "المستخدم غير موجود";
            return RedirectToAction(nameof(Users));
        }

        ViewData["Title"] = "تفاصيل المستخدم";
        ViewBag.ActiveNav = "users";
        ViewBag.CurrentUserId = _userManager.GetUserId(User);
        return View(user);
    }

    [HttpGet]
    public IActionResult CreateAdmin()
    {
        ViewData["Title"] = "إنشاء مدير جديد";
        ViewBag.ActiveNav = "create-admin";
        return View(new CreateAdminViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateAdmin(CreateAdminViewModel model)
    {
        ViewData["Title"] = "إنشاء مدير جديد";
        ViewBag.ActiveNav = "create-admin";

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = new ApplicationUser
        {
            FullName = model.FullName,
            Email = model.Email,
        };

        var result = await _userService.CreateUserAsync(user, model.Password, UserRole.Admin);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error);
            }
            return View(model);
        }

        TempData["SuccessMessage"] = $"تم إنشاء حساب المدير \"{model.FullName}\" بنجاح.";
        return RedirectToAction(nameof(Users), new { role = UserRole.Admin });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleActive(string id, bool targetStatus, string? returnUrl)
    {
        var currentUserId = _userManager.GetUserId(User);
        if (id == currentUserId)
        {
            TempData["ErrorMessage"] = "لا يمكنك تغيير حالة حسابك الخاص.";
            return LocalRedirectOrDefault(returnUrl, nameof(Users));
        }

        var result = await _userService.SetActiveStatusAsync(id, targetStatus);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded
                ? (targetStatus ? "تم تفعيل الحساب بنجاح." : "تم إيقاف الحساب بنجاح.")
                : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Users));
    }

    public async Task<IActionResult> Courses(string? term, bool? isPublished)
    {
        ViewData["Title"] = "الكورسات";
        ViewBag.ActiveNav = "courses";
        ViewBag.Term = term ?? "";
        ViewBag.IsPublishedFilter = isPublished is null ? "" : isPublished.Value ? "true" : "false";

        var courses = await _courseService.SearchCoursesAsync(term, isPublished);
        return View(courses);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Publish(int id, string? returnUrl)
    {
        var result = await _courseService.SetPublishedStatusAsync(id, true);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم نشر الكورس بنجاح." : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Courses));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Unpublish(int id, string? returnUrl)
    {
        var result = await _courseService.SetPublishedStatusAsync(id, false);
        TempData[result.Succeeded ? "SuccessMessage" : "ErrorMessage"] =
            result.Succeeded ? "تم إلغاء نشر الكورس." : string.Join(" ", result.Errors);

        return LocalRedirectOrDefault(returnUrl, nameof(Courses));
    }

    private IActionResult LocalRedirectOrDefault(string? returnUrl, string defaultAction)
    {
        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        return RedirectToAction(defaultAction);
    }

    // --- Withdrawals ----------------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Withdrawals(WithdrawalStatus? status)
    {
        ViewData["Title"] = "طلبات السحب";
        ViewBag.ActiveNav = "withdrawals";

        var requests = await _withdrawalService.GetAllWithdrawalsAsync(status);
        ViewBag.CurrentStatusFilter = status;
        
        return View(requests);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveWithdrawal(WithdrawalActionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "الطلب غير موجود.";
            return RedirectToAction(nameof(Withdrawals));
        }

        var adminId = _userManager.GetUserId(User)!;
        var result = await _withdrawalService.ApproveRequestAsync(model.RequestId, adminId, model.AdminNotes);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Withdrawals));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectWithdrawal(WithdrawalActionViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["ErrorMessage"] = "الطلب غير موجود.";
            return RedirectToAction(nameof(Withdrawals));
        }

        var adminId = _userManager.GetUserId(User)!;
        var result = await _withdrawalService.RejectRequestAsync(model.RequestId, adminId, model.AdminNotes);

        if (result.Success)
            TempData["SuccessMessage"] = result.Message;
        else
            TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Withdrawals));
    }

    [HttpGet]
    public async Task<IActionResult> ExportWithdrawalsExcel(WithdrawalStatus? status)
    {
        var requests = await _withdrawalService.GetAllWithdrawalsAsync(status);

        using var workbook = new ClosedXML.Excel.XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Withdrawal Declaration");

        // Headers
        worksheet.Cell(1, 1).Value = "Withdrawal ID";
        worksheet.Cell(1, 2).Value = "Teacher Name";
        worksheet.Cell(1, 3).Value = "Email";
        worksheet.Cell(1, 4).Value = "Bank Name";
        worksheet.Cell(1, 5).Value = "Account Holder";
        worksheet.Cell(1, 6).Value = "IBAN / Account Number";
        worksheet.Cell(1, 7).Value = "Requested Amount";
        worksheet.Cell(1, 8).Value = "Approved Amount";
        worksheet.Cell(1, 9).Value = "Request Date";
        worksheet.Cell(1, 10).Value = "Approval Date";
        worksheet.Cell(1, 11).Value = "Status";
        worksheet.Cell(1, 12).Value = "Admin Notes";

        var headerRow = worksheet.Row(1);
        headerRow.Style.Font.Bold = true;
        headerRow.Style.Fill.BackgroundColor = ClosedXML.Excel.XLColor.LightGray;
        worksheet.SheetView.FreezeRows(1);

        if (!requests.Any())
        {
            worksheet.Cell(2, 1).Value = "No withdrawal records found.";
            worksheet.Range(2, 1, 2, 12).Merge();
            worksheet.Cell(2, 1).Style.Alignment.Horizontal = ClosedXML.Excel.XLAlignmentHorizontalValues.Center;
        }
        else
        {
            int row = 2;
            decimal totalRequested = 0;
            decimal totalApproved = 0;

            foreach (var r in requests)
            {
                worksheet.Cell(row, 1).Value = r.Id;
                worksheet.Cell(row, 2).Value = r.TeacherName;
                worksheet.Cell(row, 3).Value = r.TeacherEmail;
                worksheet.Cell(row, 4).Value = r.BankName ?? "";
                worksheet.Cell(row, 5).Value = r.AccountName ?? "";
                worksheet.Cell(row, 6).Value = !string.IsNullOrEmpty(r.IBAN) ? r.IBAN : r.AccountNumber ?? "";
                worksheet.Cell(row, 7).Value = r.Amount;
                worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00 EGP";
                
                decimal approvedAmount = r.Status == "Approved" ? r.Amount : 0;
                worksheet.Cell(row, 8).Value = approvedAmount;
                worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 EGP";

                worksheet.Cell(row, 9).Value = r.RequestedAt;
                worksheet.Cell(row, 9).Style.DateFormat.Format = "yyyy-MM-dd hh:mm AM/PM";
                
                if (r.Status == "Approved")
                {
                    worksheet.Cell(row, 10).Value = r.RequestedAt; 
                    worksheet.Cell(row, 10).Style.DateFormat.Format = "yyyy-MM-dd hh:mm AM/PM";
                }

                worksheet.Cell(row, 11).Value = r.Status;
                worksheet.Cell(row, 12).Value = r.AdminNotes ?? "";

                totalRequested += r.Amount;
                totalApproved += approvedAmount;

                row++;
            }

            // Totals Row
            worksheet.Cell(row, 1).Value = "Totals";
            worksheet.Cell(row, 1).Style.Font.Bold = true;
            
            worksheet.Cell(row, 2).Value = $"Count: {requests.Count}";
            worksheet.Cell(row, 2).Style.Font.Bold = true;

            worksheet.Cell(row, 7).Value = totalRequested;
            worksheet.Cell(row, 7).Style.Font.Bold = true;
            worksheet.Cell(row, 7).Style.NumberFormat.Format = "#,##0.00 EGP";

            worksheet.Cell(row, 8).Value = totalApproved;
            worksheet.Cell(row, 8).Style.Font.Bold = true;
            worksheet.Cell(row, 8).Style.NumberFormat.Format = "#,##0.00 EGP";
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new System.IO.MemoryStream();
        workbook.SaveAs(stream);
        var content = stream.ToArray();

        string filename = $"Withdrawal_Declaration_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx";
        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", filename);
    }

    // --- Finance Analytics ----------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Finance(LearnNova.Models.ViewModels.Admin.AdminFinanceFilterParameters filters)
    {
        ViewData["Title"] = "المالية والأرباح";
        ViewBag.ActiveNav = "finance";

        var vm = await _financeAnalyticsService.GetAdminFinanceDashboardAsync(filters);

        // Populate SelectLists
        var dbContext = HttpContext.RequestServices.GetService(typeof(LearnNova.Data.ApplicationDbContext)) as LearnNova.Data.ApplicationDbContext;
        if (dbContext != null)
        {
            var teachers = await dbContext.Users.Where(u => u.Role == UserRole.Teacher).OrderBy(u => u.FullName).Select(u => new { u.Id, u.FullName }).ToListAsync();
            var courses = await dbContext.Courses.OrderBy(c => c.Title).Select(c => new { c.Id, c.Title }).ToListAsync();
            var subjects = await dbContext.Courses.Select(c => c.Subject).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToListAsync();
            var stages = await dbContext.Courses.Select(c => c.Stage).Distinct().Where(s => !string.IsNullOrEmpty(s)).ToListAsync();

            vm.TeachersList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(teachers, "Id", "FullName", filters.TeacherId);
            vm.CoursesList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(courses, "Id", "Title", filters.CourseId);
            vm.SubjectsList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(subjects, filters.Subject);
            vm.StagesList = new Microsoft.AspNetCore.Mvc.Rendering.SelectList(stages, filters.Stage);
        }

        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> ExportFinanceReport(string format, LearnNova.Models.ViewModels.Admin.AdminFinanceFilterParameters filters)
    {
        if (format?.ToLower() == "csv")
        {
            var csv = await _financeAnalyticsService.GenerateRevenueReportCsvAsync(filters);
            var bytes = System.Text.Encoding.UTF8.GetBytes(csv);
            return File(bytes, "text/csv", $"Admin_Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
        }
        else if (format?.ToLower() == "excel")
        {
            var excelBytes = await _financeAnalyticsService.GenerateRevenueReportExcelAsync(filters);
            return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"Admin_Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        else if (format?.ToLower() == "pdf")
        {
            var pdfBytes = await _financeAnalyticsService.GenerateRevenueReportPdfAsync(filters);
            return File(pdfBytes, "application/pdf", $"Admin_Revenue_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
        }
        
        return BadRequest("Unsupported format.");
    }

    // --- Coupons Management --------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Coupons()
    {
        ViewData["Title"] = "إدارة الكوبونات";
        ViewBag.ActiveNav = "coupons";

        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        var coupons = await couponService!.GetAllCouponsAsync();
        
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
                CourseTitle = c.Course?.Title,
                TeacherName = c.Teacher?.FullName
            });
        }

        return View(list);
    }

    [HttpGet]
    public IActionResult CreateCoupon()
    {
        ViewData["Title"] = "إدارة الكوبون";
        ViewBag.ActiveNav = "coupons";
        return View("CouponForm", new LearnNova.Models.ViewModels.Admin.CouponFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateCoupon(LearnNova.Models.ViewModels.Admin.CouponFormViewModel vm)
    {
        ViewData["Title"] = "إدارة الكوبون";
        ViewBag.ActiveNav = "coupons";

        if (!ModelState.IsValid) return View("CouponForm", vm);

        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        
        var coupon = new Coupon
        {
            Code = vm.Code,
            DiscountType = vm.DiscountType,
            DiscountValue = vm.DiscountValue,
            StartDate = vm.StartDate,
            EndDate = vm.EndDate,
            MinimumOrder = vm.MinimumOrder,
            MaximumDiscount = vm.MaximumDiscount,
            UsageLimit = vm.UsageLimit,
            CourseId = vm.CourseId,
            TeacherId = vm.TeacherId,
            IsActive = vm.IsActive
        };

        try
        {
            var result = await couponService!.CreateCouponAsync(coupon);
            if (result.Success)
            {
                TempData["SuccessMessage"] = result.Message;
                return RedirectToAction(nameof(Coupons));
            }

            ModelState.AddModelError("", result.Message);
            return View("CouponForm", vm);
        }
        catch (Exception ex)
        {
            // Log the exception (ILogger should ideally be used here)
            Console.WriteLine($"Error creating coupon: {ex.Message}");
            ModelState.AddModelError("", "حدث خطأ غير متوقع أثناء حفظ الكوبون. يرجى المحاولة مرة أخرى.");
            return View("CouponForm", vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ToggleCouponStatus(int id)
    {
        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        var result = await couponService!.ToggleCouponStatusAsync(id);
        
        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;
        
        return RedirectToAction(nameof(Coupons));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCoupon(int id)
    {
        var couponService = HttpContext.RequestServices.GetService(typeof(ICouponService)) as ICouponService;
        var result = await couponService!.DeleteCouponAsync(id);
        
        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;
        
        return RedirectToAction(nameof(Coupons));
    }

    // --- Refunds & Disputes --------------------------------------------------

    [HttpGet]
    public async Task<IActionResult> Refunds(LearnNova.Models.Enums.RefundStatus? statusFilter)
    {
        ViewData["Title"] = "إدارة المرتجعات";
        ViewBag.ActiveNav = "refunds";

        var refundService = HttpContext.RequestServices.GetService(typeof(IRefundService)) as IRefundService;
        var refunds = await refundService!.GetAllRefundsAsync(statusFilter);

        ViewBag.CurrentFilter = statusFilter;
        return View(refunds);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveRefund(int id, string? adminNotes)
    {
        var refundService = HttpContext.RequestServices.GetService(typeof(IRefundService)) as IRefundService;
        var result = await refundService!.ApproveRefundAsync(id, adminNotes);

        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Refunds));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RejectRefund(int id, string? adminNotes)
    {
        var refundService = HttpContext.RequestServices.GetService(typeof(IRefundService)) as IRefundService;
        var result = await refundService!.RejectRefundAsync(id, adminNotes);

        if (result.Success) TempData["SuccessMessage"] = result.Message;
        else TempData["ErrorMessage"] = result.Message;

        return RedirectToAction(nameof(Refunds));
    }
}
