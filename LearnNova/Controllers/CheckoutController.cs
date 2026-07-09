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
public class CheckoutController : Controller
{
    private readonly ICheckoutService _checkoutService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IPaymentService _paymentService;

    public CheckoutController(ICheckoutService checkoutService, UserManager<ApplicationUser> userManager, IPaymentService paymentService)
    {
        _checkoutService = checkoutService;
        _userManager = userManager;
        _paymentService = paymentService;
    }

    [HttpGet]
    public async Task<IActionResult> Index(int courseId, string? couponCode = null)
    {
        var studentId = _userManager.GetUserId(User);
        if (studentId == null) return Unauthorized();

        try
        {
            var vm = await _checkoutService.PrepareCheckoutAsync(courseId, studentId, couponCode);
            return View(vm);
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction("Details", "Course", new { id = courseId });
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyCoupon(int courseId, string? couponCode)
    {
        // Simple postback to index with the coupon code to re-evaluate pricing.
        return RedirectToAction(nameof(Index), new { courseId, couponCode });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Process(int courseId, string paymentMethod, string? couponCode)
    {
        var studentId = _userManager.GetUserId(User);
        if (studentId == null) return Unauthorized();

        var result = await _checkoutService.ProcessCheckoutAsync(courseId, studentId, paymentMethod, couponCode);

        if (result.Success)
        {
            return RedirectToAction(nameof(Success), new { paymentId = result.PaymentId });
        }
        else
        {
            return RedirectToAction(nameof(Failed), new { paymentId = result.PaymentId, error = result.ErrorMessage });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Success(int paymentId)
    {
        var payment = await _paymentService.GetPaymentAsync(paymentId);
        if (payment == null) return NotFound();

        var vm = new CheckoutSuccessViewModel
        {
            CourseId = payment.CourseId,
            CourseTitle = payment.Course?.Title ?? "Course",
            TransactionId = payment.TransactionId ?? "N/A",
            InvoiceNumber = payment.Invoice?.InvoiceNumber ?? "N/A"
        };
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Failed(int paymentId, string error)
    {
        var payment = await _paymentService.GetPaymentAsync(paymentId);
        if (payment == null) return NotFound();

        var vm = new CheckoutFailedViewModel
        {
            CourseId = payment.CourseId,
            CourseTitle = payment.Course?.Title ?? "Course",
            ErrorMessage = error ?? "Payment processing failed."
        };
        return View(vm);
    }
}
