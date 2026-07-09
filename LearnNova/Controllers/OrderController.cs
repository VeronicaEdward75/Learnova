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
    public async Task<IActionResult> Wallet()
    {
        ViewData["Title"] = "سجل الطلبات";
        ViewBag.ActiveNav = "student-wallet";
        var studentId = _userManager.GetUserId(User)!;
        var wallet = await _walletService.GetWalletAsync(studentId);
        var transactions = await _walletService.GetTransactionsAsync(wallet.Id);
        ViewBag.Transactions = transactions;
        return View(wallet);
    }

    [HttpGet]
    public async Task<IActionResult> Refunds()
    {
        ViewData["Title"] = "طلبات الاسترداد";
        ViewBag.ActiveNav = "refunds";
        var studentId = _userManager.GetUserId(User)!;

        var refunds = await _refundService.GetStudentRefundRequestsAsync(studentId);
        return View(refunds);
    }

    [HttpGet]
    public async Task<IActionResult> History()
    {
        ViewData["Title"] = "سجل الطلبات";
        ViewBag.ActiveNav = "orders";
        var studentId = _userManager.GetUserId(User)!;

        var orders = await _refundService.GetStudentOrdersAsync(studentId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Payments()
    {
        ViewData["Title"] = "المدفوعات";
        ViewBag.ActiveNav = "payments";
        var studentId = _userManager.GetUserId(User)!;

        var orders = await _refundService.GetStudentOrdersAsync(studentId);
        return View(orders);
    }

    [HttpGet]
    public async Task<IActionResult> Invoices()
    {
        ViewData["Title"] = "الفواتير";
        ViewBag.ActiveNav = "invoices";
        var studentId = _userManager.GetUserId(User)!;

        var orders = await _refundService.GetStudentOrdersAsync(studentId);
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


