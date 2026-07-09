using System;
using System.Threading.Tasks;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using LearnNova.Models.Entities;

namespace LearnNova.Controllers;

[Authorize(Roles = "Student")]
public class OrderController : Controller
{
    private readonly IRefundService _refundService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IWalletService _walletService;

    public OrderController(IRefundService refundService, UserManager<ApplicationUser> userManager, IWalletService walletService)
    {
        _refundService = refundService;
        _userManager = userManager;
        _walletService = walletService;
    }

    [HttpGet]
    public async Task<IActionResult> Wallet([FromQuery] LearnNova.Models.ViewModels.Student.Filters.WalletFilterParameters filters)
    {
        ViewData["Title"] = "المحفظة";
        ViewBag.ActiveNav = "student-wallet";
        var studentId = _userManager.GetUserId(User)!;
        var wallet = await _walletService.GetWalletAsync(studentId);

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("الأعلى قيمة", "amount_desc"),
            new("الأقل قيمة", "amount_asc")
        };
        
        filters.StatusOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("إيداع", "Deposit"),
            new("سحب", "Withdrawal"),
            new("مبيعات", "Sale"),
            new("استرداد", "Refund"),
            new("شراء", "Purchase")
        };

        var transactions = await _walletService.GetTransactionsPagedAsync(wallet.Id, filters);
        ViewBag.Transactions = transactions;
        ViewBag.Filters = filters;
        return View(wallet);
    }

    [HttpGet]
    public async Task<IActionResult> Refunds([FromQuery] LearnNova.Models.ViewModels.Student.Filters.RefundFilterParameters filters)
    {
        ViewData["Title"] = "طلبات الاسترداد";
        ViewBag.ActiveNav = "refunds";
        var studentId = _userManager.GetUserId(User)!;

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("الأعلى قيمة", "amount_desc"),
            new("الأقل قيمة", "amount_asc")
        };
        
        filters.StatusOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("قيد المراجعة", "Pending"),
            new("مقبول", "Approved"),
            new("مرفوض", "Rejected")
        };

        var refunds = await _refundService.GetStudentRefundRequestsPagedAsync(studentId, filters);
        ViewBag.Filters = filters;
        return View(refunds);
    }

    [HttpGet]
    public async Task<IActionResult> History([FromQuery] LearnNova.Models.ViewModels.Student.Filters.OrderFilterParameters filters)
    {
        ViewData["Title"] = "سجل الطلبات";
        ViewBag.ActiveNav = "orders";
        var studentId = _userManager.GetUserId(User)!;

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("الأعلى قيمة", "amount_desc"),
            new("الأقل قيمة", "amount_asc")
        };
        
        filters.StatusOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("ناجحة", "Succeeded"),
            new("قيد الانتظار", "Pending"),
            new("فاشلة", "Failed"),
            new("مستردة", "Refunded")
        };

        var orders = await _refundService.GetStudentOrdersPagedAsync(studentId, filters);
        ViewBag.Filters = filters;
        
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Payments([FromQuery] LearnNova.Models.ViewModels.Student.Filters.OrderFilterParameters filters)
    {
        ViewData["Title"] = "المدفوعات";
        ViewBag.ActiveNav = "payments";
        var studentId = _userManager.GetUserId(User)!;

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("الأعلى قيمة", "amount_desc"),
            new("الأقل قيمة", "amount_asc")
        };
        
        filters.StatusOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("ناجحة", "Succeeded"),
            new("قيد الانتظار", "Pending"),
            new("فاشلة", "Failed"),
            new("مستردة", "Refunded")
        };

        var orders = await _refundService.GetStudentOrdersPagedAsync(studentId, filters);
        ViewBag.Filters = filters;
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Invoices([FromQuery] LearnNova.Models.ViewModels.Student.Filters.OrderFilterParameters filters)
    {
        ViewData["Title"] = "الفواتير";
        ViewBag.ActiveNav = "invoices";
        var studentId = _userManager.GetUserId(User)!;

        filters.SortOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("الأحدث", "newest"),
            new("الأقدم", "oldest"),
            new("الأعلى قيمة", "amount_desc"),
            new("الأقل قيمة", "amount_asc")
        };
        
        filters.StatusOptions = new List<Microsoft.AspNetCore.Mvc.Rendering.SelectListItem>
        {
            new("ناجحة", "Succeeded"),
            new("قيد الانتظار", "Pending"),
            new("فاشلة", "Failed"),
            new("مستردة", "Refunded")
        };
        
        filters.HasInvoice = true;

        var orders = await _refundService.GetStudentOrdersPagedAsync(studentId, filters);
        ViewBag.Filters = filters;
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        ViewData["Title"] = "تفاصيل الطلب";
        ViewBag.ActiveNav = "orders";
        var studentId = _userManager.GetUserId(User)!;

        var details = await _refundService.GetOrderDetailsAsync(id, studentId);
        if (details == null) return NotFound();

        return View(details);
    }

    [HttpGet]
    public async Task<IActionResult> Invoice(int id)
    {
        ViewData["Title"] = "الفاتورة";
        ViewBag.ActiveNav = "invoices";
        var studentId = _userManager.GetUserId(User)!;

        var details = await _refundService.GetOrderDetailsAsync(id, studentId);
        if (details == null) return NotFound();

        return View(details);
    }

    [HttpGet]
    public async Task<IActionResult> RequestRefund(int paymentId)
    {
        ViewData["Title"] = "طلب استرداد المبلغ";
        ViewBag.ActiveNav = "orders";
        var studentId = _userManager.GetUserId(User)!;

        var canRequest = await _refundService.CanRequestRefundAsync(paymentId, studentId);
        if (!canRequest.Success)
        {
            TempData["ErrorMessage"] = canRequest.Message;
            return RedirectToAction(nameof(History));
        }

        var details = await _refundService.GetOrderDetailsAsync(paymentId, studentId);
        if (details == null) return NotFound();

        var vm = new RefundRequestViewModel
        {
            PaymentId = paymentId,
            CourseTitle = details.CourseTitle,
            AmountPaid = details.TotalPaid
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestRefund(RefundRequestViewModel vm)
    {
        ViewData["Title"] = "طلب استرداد المبلغ";
        ViewBag.ActiveNav = "orders";
        var studentId = _userManager.GetUserId(User)!;

        if (!ModelState.IsValid) return View(vm);

        var result = await _refundService.RequestRefundAsync(vm.PaymentId, studentId, vm.Reason, vm.Description);
        
        if (result.Success)
        {
            TempData["SuccessMessage"] = result.Message;
            return RedirectToAction(nameof(History));
        }

        ModelState.AddModelError("", result.Message);
        return View(vm);
    }
}


