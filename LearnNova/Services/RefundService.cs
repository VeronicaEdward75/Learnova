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
    private readonly IGenericRepository<WalletTransaction> _transactionRepo;
    private readonly IGenericRepository<CouponUsage> _couponUsageRepo;
    private readonly LearnNova.Data.ApplicationDbContext _dbContext;

    public RefundService(
        IGenericRepository<RefundRequest> refundRepo,
        IPaymentRepository paymentRepo,
        ISystemSettingsRepository settingsRepo,
        IEnrollmentService enrollmentService,
        IWalletService walletService,
        IGenericRepository<WalletTransaction> transactionRepo,
        IGenericRepository<CouponUsage> couponUsageRepo,
        LearnNova.Data.ApplicationDbContext dbContext)
    {
        _refundRepo = refundRepo;
        _paymentRepo = paymentRepo;
        _settingsRepo = settingsRepo;
        _enrollmentService = enrollmentService;
        _walletService = walletService;
        _transactionRepo = transactionRepo;
        _couponUsageRepo = couponUsageRepo;
        _dbContext = dbContext;
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
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var refund = await _refundRepo.GetByIdAsync(refundId);
            if (refund == null) return (false, "الطلب غير موجود");
            if (refund.Status != RefundStatus.Pending) return (false, "الطلب ليس قيد المراجعة");

            var payment = await _paymentRepo.GetQueryable()
                .Include(p => p.Course)
                .FirstOrDefaultAsync(p => p.Id == refund.PaymentId);
            if (payment == null) return (false, "عملية الدفع غير موجودة");

            if (payment.Status == PaymentStatus.Refunded) return (false, "تم استرداد هذا الطلب مسبقاً");

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
                
                if (wallet != null)
                {
                    await _walletService.CreateTransactionAsync(wallet.Id, -payment.TeacherAmount, TransactionType.Refund, $"استرداد للطالب عن كورس: {payment.Course.Title}", payment.Id);
                }
            }

            // 4. Credit Student Wallet
            if (payment.WalletUsedAmount > 0)
            {
                await _walletService.CreditWalletRefundAsync(payment.StudentId, payment.WalletUsedAmount, payment.Id);
            }
            if (payment.GatewayUsedAmount > 0)
            {
                await _walletService.CreditAvailableBalanceAsync(payment.StudentId, payment.GatewayUsedAmount, payment.Id);
            }
            if (payment.WalletUsedAmount == 0 && payment.GatewayUsedAmount == 0 && payment.StudentPaid > 0)
            {
                // Fallback for older payments before hybrid payment tracking
                await _walletService.CreditAvailableBalanceAsync(payment.StudentId, payment.StudentPaid, payment.Id);
            }

            // 5. Update Refund Request
            refund.Status = RefundStatus.Approved;
            refund.AdminNotes = adminNotes;
            refund.ResolvedAt = DateTime.UtcNow;
            _refundRepo.Update(refund);
            await _refundRepo.SaveChangesAsync();

            await transaction.CommitAsync();
            return (true, "تم الموافقة على الاسترداد بنجاح");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return (false, $"حدث خطأ أثناء معالجة الاسترداد: {ex.Message}");
        }
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

    public async Task<List<StudentRefundViewModel>> GetStudentRefundRequestsAsync(string studentId)
    {
        var requests = await _refundRepo.GetQueryable()
            .Include(r => r.Payment)
            .ThenInclude(p => p.Course)
            .Where(r => r.StudentId == studentId)
            .OrderByDescending(r => r.RequestedAt)
            .ToListAsync();

        return requests.Select(r => new StudentRefundViewModel
        {
            RefundId = r.Id,
            CourseTitle = r.Payment?.Course?.Title ?? "N/A",
            RequestDate = r.RequestedAt,
            RefundAmount = r.Payment?.StudentPaid ?? 0,
            Reason = r.Reason,
            AdminNotes = r.AdminNotes,
            Status = r.Status
        }).ToList();
    }

    public async Task<LearnNova.Models.ViewModels.PagedResult<StudentRefundViewModel>> GetStudentRefundRequestsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.RefundFilterParameters filters)
    {
        var query = _refundRepo.GetQueryable()
            .Include(r => r.Payment)
            .ThenInclude(p => p.Course)
            .Where(r => r.StudentId == studentId);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query = query.Where(r => r.Payment != null && r.Payment.Course != null && r.Payment.Course.Title.Contains(filters.SearchTerm));
        }

        if (filters.StartDate.HasValue)
        {
            query = query.Where(r => r.RequestedAt >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            query = query.Where(r => r.RequestedAt <= filters.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            if (Enum.TryParse<RefundStatus>(filters.Status, true, out var statusEnum))
            {
                query = query.Where(r => r.Status == statusEnum);
            }
        }

        query = filters.SortBy switch
        {
            "oldest" => query.OrderBy(r => r.RequestedAt),
            "amount_desc" => query.OrderByDescending(r => r.Payment != null ? r.Payment.StudentPaid : 0),
            "amount_asc" => query.OrderBy(r => r.Payment != null ? r.Payment.StudentPaid : 0),
            _ => query.OrderByDescending(r => r.RequestedAt)
        };

        var totalCount = await query.CountAsync();
        
        int pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        int pageSize = filters.PageSize > 0 ? filters.PageSize : 20;

        var pagedRequests = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var list = pagedRequests.Select(r => new StudentRefundViewModel
        {
            RefundId = r.Id,
            CourseTitle = r.Payment?.Course?.Title ?? "N/A",
            RequestDate = r.RequestedAt,
            RefundAmount = r.Payment?.StudentPaid ?? 0,
            Reason = r.Reason,
            AdminNotes = r.AdminNotes,
            Status = r.Status
        }).ToList();

        return new LearnNova.Models.ViewModels.PagedResult<StudentRefundViewModel>
        {
            Items = list,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<List<OrderHistoryViewModel>> GetStudentOrdersAsync(string studentId)
    {
        var payments = await _paymentRepo.GetQueryable()
            .Include(p => p.Course)
            .Include(p => p.Invoice)
            .Where(p => p.StudentId == studentId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        var list = new List<OrderHistoryViewModel>();
        foreach (var p in payments)
        {
            var vm = new OrderHistoryViewModel
            {
                PaymentId = p.Id,
                CourseId = p.CourseId,
                CourseTitle = p.Course?.Title ?? "N/A",
                PurchaseDate = p.CreatedAt,
                AmountPaid = p.StudentPaid,
                Status = p.Status,
                InvoiceNumber = p.Invoice?.InvoiceNumber
            };

            var existingRefunds = await _refundRepo.FindAsync(r => r.PaymentId == p.Id);
            var refund = existingRefunds.FirstOrDefault(r => r.Status == RefundStatus.Pending || r.Status == RefundStatus.Approved || r.Status == RefundStatus.Rejected);
            
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

    public async Task<LearnNova.Models.ViewModels.PagedResult<OrderHistoryViewModel>> GetStudentOrdersPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.OrderFilterParameters filters)
    {
        var query = _paymentRepo.GetQueryable()
            .Include(p => p.Course)
            .Include(p => p.Invoice)
            .Where(p => p.StudentId == studentId);

        if (!string.IsNullOrWhiteSpace(filters.SearchTerm))
        {
            query = query.Where(p => p.Course != null && p.Course.Title.Contains(filters.SearchTerm) || (p.Invoice != null && p.Invoice.InvoiceNumber.Contains(filters.SearchTerm)));
        }

        if (filters.StartDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            query = query.Where(p => p.CreatedAt <= filters.EndDate.Value);
        }

        if (filters.HasInvoice.HasValue && filters.HasInvoice.Value)
        {
            query = query.Where(p => p.Invoice != null && p.Invoice.InvoiceNumber != null);
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            if (Enum.TryParse<PaymentStatus>(filters.Status, true, out var statusEnum))
            {
                query = query.Where(p => p.Status == statusEnum);
            }
        }

        query = filters.SortBy switch
        {
            "oldest" => query.OrderBy(p => p.CreatedAt),
            "amount_desc" => query.OrderByDescending(p => p.StudentPaid),
            "amount_asc" => query.OrderBy(p => p.StudentPaid),
            _ => query.OrderByDescending(p => p.CreatedAt)
        };

        var totalCount = await query.CountAsync();
        
        int pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        int pageSize = filters.PageSize > 0 ? filters.PageSize : 20;

        var pagedPayments = await query.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

        var list = new List<OrderHistoryViewModel>();
        foreach (var p in pagedPayments)
        {
            var vm = new OrderHistoryViewModel
            {
                PaymentId = p.Id,
                CourseId = p.CourseId,
                CourseTitle = p.Course?.Title ?? "N/A",
                PurchaseDate = p.CreatedAt,
                AmountPaid = p.StudentPaid,
                Status = p.Status,
                InvoiceNumber = p.Invoice?.InvoiceNumber
            };

            var existingRefunds = await _refundRepo.FindAsync(r => r.PaymentId == p.Id);
            var refund = existingRefunds.FirstOrDefault(r => r.Status == RefundStatus.Pending || r.Status == RefundStatus.Approved || r.Status == RefundStatus.Rejected);
            
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

        return new LearnNova.Models.ViewModels.PagedResult<OrderHistoryViewModel>
        {
            Items = list,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
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
        var couponUsage = (await _couponUsageRepo.GetQueryable().Include(cu => cu.Coupon).FirstOrDefaultAsync(u => u.PaymentId == paymentId));
        var coupon = couponUsage?.Coupon;
        var canRequestRefund = await CanRequestRefundAsync(paymentId, studentId);

        decimal originalPrice = payment.Course?.Price ?? 0;
        decimal discountAmount = 0;
        decimal discountPercentage = 0;
        
        if (coupon != null)
        {
            if (coupon.DiscountType == LearnNova.Models.Enums.DiscountType.Percentage)
            {
                discountPercentage = coupon.DiscountValue;
                discountAmount = originalPrice * (discountPercentage / 100m);
            }
            else
            {
                discountAmount = coupon.DiscountValue;
                if (originalPrice > 0)
                    discountPercentage = (discountAmount / originalPrice) * 100m;
            }
        }
        else
        {
            // Fallback backward calculation if coupon is lost but price was discounted
            decimal expectedTotalWithoutVat = originalPrice + payment.PlatformFee;
            if (payment.StudentPaid < expectedTotalWithoutVat && originalPrice > 0)
            {
                discountAmount = expectedTotalWithoutVat - payment.StudentPaid; // Approximation if no vat was applied
            }
        }

        // Assuming VAT isn't stored, we can approximate or set to 0.
        // Actually, if StudentPaid = OriginalPrice - Discount + PlatformFee + VAT
        // VAT = StudentPaid - OriginalPrice + Discount - PlatformFee
        decimal vatAmount = payment.StudentPaid - originalPrice + discountAmount - payment.PlatformFee;
        if (vatAmount < 0) vatAmount = 0;

        return new OrderDetailsViewModel
        {
            PaymentId = payment.Id,
            StudentName = payment.Student?.FullName ?? "",
            StudentEmail = payment.Student?.Email ?? "",
            TeacherName = payment.Course?.Teacher?.FullName ?? "",
            CourseId = payment.CourseId,
            CourseTitle = payment.Course?.Title ?? "",
            CourseCategory = payment.Course?.Subject ?? "غير محدد",
            CourseLevel = payment.Course?.Stage ?? "غير محدد",
            AccessType = "مدى الحياة", // Example default
            
            InvoiceNumber = payment.Invoice?.InvoiceNumber,
            PurchaseDate = payment.CreatedAt,
            PaymentMethod = payment.PaymentMethod ?? "N/A",
            TransactionId = payment.TransactionId,
            
            PaymentStatus = payment.Status,
            OriginalPrice = originalPrice,
            DiscountAmount = discountAmount, 
            PlatformFee = payment.PlatformFee,
            VatAmount = vatAmount,
            WalletPaid = payment.WalletUsedAmount,
            GatewayPaid = payment.GatewayUsedAmount,
            TotalPaid = payment.StudentPaid,
            
            CouponCode = coupon?.Code,
            CouponDiscountPercentage = discountPercentage,
            CouponDiscountType = coupon?.DiscountType.ToString(),
            CouponDiscountValue = coupon?.DiscountValue ?? 0,
            
            CanRequestRefund = canRequestRefund.Success,
            HasRefundRequest = refund != null,
            RefundRequestedAt = refund?.RequestedAt,
            RefundDate = refund?.ResolvedAt,
            RefundAmount = refund != null ? (payment.WalletUsedAmount + payment.GatewayUsedAmount) : 0, // Fallback if no exact sum stored in refund row
            RefundStatus = refund?.Status,
            RefundReason = refund?.Reason,
            RefundAdminNotes = refund?.AdminNotes
        };
    }

    public async Task<List<AdminRefundViewModel>> GetAllRefundsAsync(RefundStatus? statusFilter = null)
    {
        var query = _refundRepo.GetQueryable()
            .Include(r => r.Payment)
                .ThenInclude(p => p.Course)
                    .ThenInclude(c => c.Teacher)
            .Include(r => r.Student)
            .AsQueryable();
            
        if (statusFilter.HasValue)
        {
            query = query.Where(r => r.Status == statusFilter.Value);
        }

        var refunds = await query.OrderByDescending(r => r.RequestedAt).ToListAsync();

        return refunds.Select(r => new AdminRefundViewModel
        {
            RefundId = r.Id,
            PaymentId = r.PaymentId,
            StudentName = r.Student?.FullName ?? "N/A",
            CourseTitle = r.Payment?.Course?.Title ?? "N/A",
            TeacherName = r.Payment?.Course?.Teacher?.FullName ?? "N/A",
            Amount = r.Payment?.StudentPaid ?? 0,
            TeacherLoss = r.Payment?.TeacherAmount ?? 0,
            PlatformLoss = r.Payment?.PlatformFee ?? 0,
            StudentRefund = r.Payment?.StudentPaid ?? 0,
            Reason = r.Reason,
            Description = r.Description,
            AdminNotes = r.AdminNotes,
            Status = r.Status,
            RequestedAt = r.RequestedAt,
            ResolvedAt = r.ResolvedAt
        }).ToList();
    }
}

