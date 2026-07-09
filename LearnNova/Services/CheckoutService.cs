using System;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Repositories;

namespace LearnNova.Services;

public interface ICheckoutService
{
    Task<CheckoutViewModel> PrepareCheckoutAsync(int courseId, string studentId, string? couponCode = null);
    Task<(bool Success, string TransactionId, string ErrorMessage, int PaymentId)> ProcessCheckoutAsync(int courseId, string studentId, string paymentMethod, string? couponCode = null);
}

public class CheckoutService : ICheckoutService
{
    private readonly ICourseRepository _courseRepository;
    private readonly IEnrollmentService _enrollmentService;
    private readonly ISystemSettingsRepository _systemSettingsRepository;
    private readonly IPaymentService _paymentService;
    private readonly IPaymentGateway _paymentGateway;
    private readonly IInvoiceService _invoiceService;
    private readonly IWalletService _walletService;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICouponService _couponService;

    public CheckoutService(
        ICourseRepository courseRepository,
        IEnrollmentService enrollmentService,
        ISystemSettingsRepository systemSettingsRepository,
        IPaymentService paymentService,
        IPaymentGateway paymentGateway,
        IInvoiceService invoiceService,
        IWalletService walletService,
        IPaymentRepository paymentRepository,
        ICouponService couponService)
    {
        _courseRepository = courseRepository;
        _enrollmentService = enrollmentService;
        _systemSettingsRepository = systemSettingsRepository;
        _paymentService = paymentService;
        _paymentGateway = paymentGateway;
        _invoiceService = invoiceService;
        _walletService = walletService;
        _paymentRepository = paymentRepository;
        _couponService = couponService;
    }

    public async Task<CheckoutViewModel> PrepareCheckoutAsync(int courseId, string studentId, string? couponCode = null)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || !course.IsPublished)
            throw new Exception("Course is not available for purchase.");

        if (course.TeacherId == studentId)
            throw new Exception("You cannot purchase your own course.");

        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, courseId);
        if (isEnrolled)
            throw new Exception("You are already enrolled in this course.");

        // Fallback defaults
        decimal commissionPercentage = 15.0m;
        decimal vatPercentage = 14.0m;

        var settings = (await _systemSettingsRepository.GetAllAsync()).FirstOrDefault();
        if (settings != null)
        {
            commissionPercentage = settings.PlatformCommissionPercentage;
            vatPercentage = settings.VatPercentage;
        }

        decimal originalPrice = course.Price;
        decimal discountAmount = 0;
        string? couponError = null;
        Coupon? appliedCoupon = null;

        if (!string.IsNullOrEmpty(couponCode))
        {
            var valResult = await _couponService.ValidateCouponAsync(couponCode, courseId, studentId);
            if (valResult.IsValid && valResult.Coupon != null)
            {
                appliedCoupon = valResult.Coupon;
                discountAmount = _couponService.CalculateDiscount(originalPrice, valResult.Coupon);
            }
            else
            {
                couponError = valResult.Message;
            }
        }

        decimal discountedPrice = originalPrice - discountAmount;
        if (discountedPrice < 0) discountedPrice = 0;

        decimal platformFee = discountedPrice * (commissionPercentage / 100m);
        decimal vatAmount = (discountedPrice + platformFee) * (vatPercentage / 100m);
        decimal total = discountedPrice + platformFee + vatAmount;

        int? remainingUsage = null;
        if (appliedCoupon != null && appliedCoupon.UsageLimit.HasValue)
        {
            var usageCount = await _couponService.GetCouponUsageCountAsync(appliedCoupon.Id);
            remainingUsage = appliedCoupon.UsageLimit.Value - usageCount;
        }

        return new CheckoutViewModel
        {
            CourseId = courseId,
            CourseTitle = course.Title,
            TeacherName = course.Teacher?.FullName ?? "Instructor",
            Price = originalPrice, // Original price
            DiscountAmount = discountAmount,
            FinalTotal = total,
            Total = total,
            PlatformFee = platformFee,
            VatAmount = vatAmount,
            CouponCode = appliedCoupon?.Code,
            CouponError = couponError,
            RemainingUsage = remainingUsage
        };
    }

    public async Task<(bool Success, string TransactionId, string ErrorMessage, int PaymentId)> ProcessCheckoutAsync(int courseId, string studentId, string paymentMethod, string? couponCode = null)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || !course.IsPublished)
            return (false, "", "Course not available.", 0);

        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, courseId);
        if (isEnrolled)
            return (false, "", "Already enrolled.", 0);

        var vm = await PrepareCheckoutAsync(courseId, studentId, couponCode);

        // If a coupon was provided but it became invalid, stop checkout.
        if (!string.IsNullOrEmpty(couponCode) && !string.IsNullOrEmpty(vm.CouponError))
        {
            return (false, "", vm.CouponError, 0);
        }

        // Create Payment
        var payment = await _paymentService.CreatePaymentAsync(courseId, studentId);
        
        // Use repo directly to populate full properties
        var paymentEntity = await _paymentRepository.GetByIdAsync(payment.Id);
        if (paymentEntity != null)
        {
            paymentEntity.StudentPaid = vm.FinalTotal; // The actual money student pays
            paymentEntity.TeacherAmount = vm.Price - vm.DiscountAmount; // Teacher's cut is based on discounted course price
            paymentEntity.PlatformFee = vm.PlatformFee;
            paymentEntity.GatewayFee = 0; // Simulated
            paymentEntity.Currency = vm.Currency;
            paymentEntity.Gateway = "FakeGateway";
            paymentEntity.PaymentMethod = paymentMethod;
            _paymentRepository.Update(paymentEntity);
            await _paymentRepository.SaveChangesAsync();
        }

        // Bypassing payment gateway if total is 0.00
        if (vm.FinalTotal == 0)
        {
            var freeTransactionId = $"FREE-{Guid.NewGuid().ToString().Substring(0, 8)}";
            await _paymentService.MarkSucceededAsync(payment.Id, freeTransactionId);
            await _enrollmentService.EnrollAsync(studentId, courseId);
            await _invoiceService.CreateInvoiceAsync(payment.Id);
            
            // Teacher gets 0 if it was a free coupon, but if the platform covers it, 
            // maybe we'd still add something. Assuming the coupon just discounts it:
            decimal teacherEarnings = vm.Price - vm.DiscountAmount;
            if (teacherEarnings > 0)
            {
                await _walletService.AddPendingBalanceAsync(course.TeacherId, teacherEarnings);
                var wallet = await _walletService.GetWalletAsync(course.TeacherId);
                await _walletService.CreateTransactionAsync(wallet.Id, teacherEarnings, TransactionType.Sale, $"Course Sale (Discounted): {course.Title}", payment.Id);
            }

            // Record Coupon Usage
            if (!string.IsNullOrEmpty(vm.CouponCode))
            {
                var couponObj = await _couponService.GetCouponByCodeAsync(vm.CouponCode);
                if (couponObj != null)
                {
                    await _couponService.RecordCouponUsageAsync(couponObj.Id, studentId, payment.Id);
                }
            }

            return (true, freeTransactionId, "", payment.Id);
        }

        // Call Gateway
        var gatewayResult = await _paymentGateway.ProcessPaymentAsync(paymentEntity!);

        if (gatewayResult.IsSuccess)
        {
            await _paymentService.MarkSucceededAsync(payment.Id, gatewayResult.TransactionId);

            // Create Enrollment
            await _enrollmentService.EnrollAsync(studentId, courseId);

            // Generate Invoice
            await _invoiceService.CreateInvoiceAsync(payment.Id);

            // Update Teacher Wallet
            decimal teacherEarnings = vm.Price - vm.DiscountAmount;
            if (teacherEarnings > 0)
            {
                await _walletService.AddPendingBalanceAsync(course.TeacherId, teacherEarnings);
                var wallet = await _walletService.GetWalletAsync(course.TeacherId);
                await _walletService.CreateTransactionAsync(wallet.Id, teacherEarnings, TransactionType.Sale, $"Course Sale: {course.Title}", payment.Id);
            }

            // Record Coupon Usage
            if (!string.IsNullOrEmpty(vm.CouponCode))
            {
                var couponObj = await _couponService.GetCouponByCodeAsync(vm.CouponCode);
                if (couponObj != null)
                {
                    await _couponService.RecordCouponUsageAsync(couponObj.Id, studentId, payment.Id);
                }
            }

            return (true, gatewayResult.TransactionId, "", payment.Id);
        }
        else
        {
            await _paymentService.MarkFailedAsync(payment.Id, gatewayResult.GatewayResponse);
            return (false, "", gatewayResult.GatewayResponse, payment.Id);
        }
    }
}

