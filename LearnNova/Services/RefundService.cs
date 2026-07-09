using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Admin;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class RefundService : IRefundService
{
    private readonly IGenericRepository<RefundRequest> _refundRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly ISystemSettingsRepository _settingsRepo;
    private readonly IEnrollmentService _enrollmentService;
    private readonly IWalletService _walletService;
    private readonly IGenericRepository<WalletTransaction> _transactionRepo; // Assuming we need to insert direct refund log or WalletService handles it.
    private readonly IGenericRepository<CouponUsage> _couponUsageRepo;

    public RefundService(
        IGenericRepository<RefundRequest> refundRepo,
        IPaymentRepository paymentRepo,
        ISystemSettingsRepository settingsRepo,
        IEnrollmentService enrollmentService,
        IWalletService walletService,
        IGenericRepository<WalletTransaction> transactionRepo,
        IGenericRepository<CouponUsage> couponUsageRepo)
    {
        _refundRepo = refundRepo;
        _paymentRepo = paymentRepo;
        _settingsRepo = settingsRepo;
        _enrollmentService = enrollmentService;
        _walletService = walletService;
        _transactionRepo = transactionRepo;
        _couponUsageRepo = couponUsageRepo;
    }

    public async Task<(bool Success, string Message)> CanRequestRefundAsync(int paymentId, string studentId)
    {
        var payment = await _paymentRepo.GetByIdAsync(paymentId);
        if (payment == null || payment.StudentId != studentId) return (false, "عملية الدفع غير موجودة");

        if (payment.Status != PaymentStatus.Succeeded) return (false, "لا يمكن استرداد عملية غير ناجحة");

        var settings = (await _settingsRepo.GetAllAsync()).FirstOrDefault();
        int refundDays = settings?.RefundDays ?? 14;

        if ((DateTime.UtcNow - payment.CreatedAt).TotalDays > refundDays)
            return (false, $"لقد تجاوزت فترة الاسترداد المسموحة ({refundDays} يوماً)");

        var existingRefunds = await _refundRepo.FindAsync(r => r.PaymentId == paymentId);
        var activeRefund = existingRefunds.FirstOrDefault(r => r.Status == RefundStatus.Pending || r.Status == RefundStatus.Approved);
        
        if (activeRefund != null)
        {
            if (activeRefund.Status == RefundStatus.Approved) return (false, "تم استرداد هذا الطلب مسبقاً");
            return (false, "يوجد طلب استرداد قيد المراجعة حالياً");
        }

        return (true, "مسموح بطلب استرداد");
    }

    public async Task<(bool Success, string Message)> RequestRefundAsync(int paymentId, string studentId, string reason, string? description)
    {
        var canRequest = await CanRequestRefundAsync(paymentId, studentId);
        if (!canRequest.Success) return canRequest;

        var request = new RefundRequest
        {
            PaymentId = paymentId,
            StudentId = studentId,
            Reason = reason,
            Description = description,
            Status = RefundStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        await _refundRepo.AddAsync(request);
        await _refundRepo.SaveChangesAsync();

        return (true, "تم تقديم طلب الاسترداد بنجاح وجاري مراجعته");
    }

    public async Task<(bool Success, string Message)> ApproveRefundAsync(int refundId, string? adminNotes)
    {
        var refund = await _refundRepo.GetByIdAsync(refundId);
        if (refund == null) return (false, "الطلب غير موجود");
        if (refund.Status != RefundStatus.Pending) return (false, "الطلب ليس قيد المراجعة");

        var payment = await _paymentRepo.GetQueryable()
            .Include(p => p.Course)
            .FirstOrDefaultAsync(p => p.Id == refund.PaymentId);
        if (payment == null) return (false, "عملية الدفع غير موجودة");

        // 1. Update Payment Status
        payment.Status = PaymentStatus.Refunded;
        payment.RefundedAt = DateTime.UtcNow;
        _paymentRepo.Update(payment);
        await _paymentRepo.SaveChangesAsync();

        // 2. Remove Enrollment
        await _enrollmentService.UnenrollAsync(payment.StudentId, payment.CourseId);

        // 3. Deduct Teacher Wallet
        if (payment.TeacherAmount > 0)
        {
            var wallet = await _walletService.GetWalletAsync(payment.Course.TeacherId);
            await _walletService.ProcessRefundDeductionAsync(payment.Course.TeacherId, payment.TeacherAmount);
            
            // Log negative transaction
            if (payment.TeacherAmount > 0 && wallet != null)
            {
                await _walletService.CreateTransactionAsync(wallet.Id, -payment.TeacherAmount, TransactionType.Refund, $"استرداد للطالب عن كورس: {payment.Course.Title}", payment.Id);
            }
        }

        // 4. Update Refund Request
        refund.Status = RefundStatus.Approved;
        refund.AdminNotes = adminNotes;
        refund.ResolvedAt = DateTime.UtcNow;
        _refundRepo.Update(refund);
        await _refundRepo.SaveChangesAsync();

        return (true, "تم الموافقة على الاسترداد بنجاح");
    }

    public async Task<(bool Success, string Message)> RejectRefundAsync(int refundId, string? adminNotes)
    {
        var refund = await _refundRepo.GetByIdAsync(refundId);
        if (refund == null) return (false, "الطلب غير موجود");
        if (refund.Status != RefundStatus.Pending) return (false, "الطلب ليس قيد المراجعة");

        refund.Status = RefundStatus.Rejected;
        refund.AdminNotes = adminNotes;
        refund.ResolvedAt = DateTime.UtcNow;
        
        _refundRepo.Update(refund);
        await _refundRepo.SaveChangesAsync();

        return (true, "تم رفض طلب الاسترداد");
    }

    public async Task<List<OrderHistoryViewModel>> GetStudentOrdersAsync(string studentId)
    {
        var payments = await _paymentRepo.GetQueryable().Include(p => p.Course).Include(p => p.Invoice).Where(p => p.StudentId == studentId).ToListAsync();
        var allRefunds = await _refundRepo.FindAsync(r => r.StudentId == studentId);
        var allCoupons = await _couponUsageRepo.FindAsync(u => u.StudentId == studentId);
        
        var list = new List<OrderHistoryViewModel>();

        foreach (var p in payments.OrderByDescending(x => x.CreatedAt))
        {
            var refund = allRefunds.FirstOrDefault(r => r.PaymentId == p.Id);
            var coupon = allCoupons.FirstOrDefault(c => c.PaymentId == p.Id)?.Coupon?.Code;
            
            var vm = new OrderHistoryViewModel
            {
                PaymentId = p.Id,
                CourseTitle = p.Course?.Title ?? "N/A",
                InvoiceNumber = p.Invoice?.InvoiceNumber,
                PurchaseDate = p.CreatedAt,
                AmountPaid = p.StudentPaid, // Since Phase 13.6, StudentPaid is the final total
                Currency = p.Currency,
                Status = p.Status,
                CouponCode = coupon,
                TransactionId = p.TransactionId,
                PaymentMethod = p.PaymentMethod,
                CanRequestRefund = false
            };

            if (refund != null)
            {
                vm.RefundStatusMessage = refund.Status switch
                {
                    RefundStatus.Pending => "طلب استرداد قيد المراجعة",
                    RefundStatus.Approved => "تم الاسترداد",
                    RefundStatus.Rejected => "تم رفض الاسترداد",
                    _ => ""
                };
            }
            else
            {
                var canReq = await CanRequestRefundAsync(p.Id, studentId);
                vm.CanRequestRefund = canReq.Success;
            }

            list.Add(vm);
        }

        return list;
    }

    public async Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int paymentId, string studentId)
    {
        var payment = await _paymentRepo.GetQueryable()
            .Include(p => p.Student)
            .Include(p => p.Course).ThenInclude(c => c.Teacher)
            .Include(p => p.Invoice)
            .FirstOrDefaultAsync(p => p.Id == paymentId && p.StudentId == studentId);
            
        if (payment == null) return null;

        var refund = (await _refundRepo.FindAsync(r => r.PaymentId == paymentId)).FirstOrDefault();
        var coupon = (await _couponUsageRepo.FindAsync(u => u.PaymentId == paymentId)).FirstOrDefault()?.Coupon?.Code;
        var canRequestRefund = await CanRequestRefundAsync(paymentId, studentId);

        return new OrderDetailsViewModel
        {
            PaymentId = payment.Id,
            StudentName = payment.Student?.FullName ?? "",
            TeacherName = payment.Course?.Teacher?.FullName ?? "",
            CourseId = payment.CourseId,
            CourseTitle = payment.Course?.Title ?? "",
            
            InvoiceNumber = payment.Invoice?.InvoiceNumber,
            PurchaseDate = payment.CreatedAt,
            PaymentMethod = payment.PaymentMethod ?? "N/A",
            TransactionId = payment.TransactionId,
            OriginalPrice = payment.TeacherAmount + payment.PlatformFee, // Approximation based on logic
            DiscountAmount = 0, // Since we didn't store discount directly on Payment, we'll leave it 0 or calculate it
            PlatformFee = payment.PlatformFee,
            VatAmount = 0, // Not stored directly
            TotalPaid = payment.StudentPaid,
            Currency = payment.Currency,
            CouponCode = coupon,
            PaymentStatus = payment.Status,
            HasRefundRequest = refund != null,
            RefundStatus = refund?.Status,
            RefundReason = refund?.Reason,
            RefundAdminNotes = refund?.AdminNotes,
            RefundRequestedAt = refund?.RequestedAt,
            CanRequestRefund = canRequestRefund.Success
        };
    }

    public async Task<List<AdminRefundViewModel>> GetAllRefundsAsync(RefundStatus? statusFilter = null)
    {
        var refunds = await _refundRepo.GetAllAsync();
        if (statusFilter.HasValue)
        {
            refunds = refunds.Where(r => r.Status == statusFilter.Value);
        }

        return refunds.OrderByDescending(r => r.RequestedAt).Select(r => new AdminRefundViewModel
        {
            RefundId = r.Id,
            PaymentId = r.PaymentId,
            StudentName = r.Student?.FullName ?? "N/A",
            CourseTitle = r.Payment?.Course?.Title ?? "N/A",
            TeacherName = r.Payment?.Course?.Teacher?.FullName ?? "N/A",
            Amount = r.Payment?.StudentPaid ?? 0,
            Reason = r.Reason,
            Description = r.Description,
            AdminNotes = r.AdminNotes,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            ResolvedAt = r.ResolvedAt
        }).ToList();
    }
}



