using System;
using System.ComponentModel.DataAnnotations;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Student;

public class OrderHistoryViewModel
{
    public int PaymentId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public string? InvoiceNumber { get; set; }
    public DateTime PurchaseDate { get; set; }
    public decimal AmountPaid { get; set; }
    public string Currency { get; set; } = "EGP";
    public PaymentStatus Status { get; set; }
    public string? CouponCode { get; set; }
    
    // For payments
    public string? TransactionId { get; set; }
    public string? PaymentMethod { get; set; }
    
    // For refund logic
    public bool CanRequestRefund { get; set; }
    public string? RefundStatusMessage { get; set; }
}

public class OrderDetailsViewModel
{
    public int PaymentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public string? InvoiceNumber { get; set; }
    
    public DateTime PurchaseDate { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? TransactionId { get; set; }
    
    public decimal OriginalPrice { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal VatAmount { get; set; }
    public decimal TotalPaid { get; set; }
    public string Currency { get; set; } = "EGP";
    
    public string? CouponCode { get; set; }
    
    public PaymentStatus PaymentStatus { get; set; }
    
    // Refund
    public bool HasRefundRequest { get; set; }
    public DateTime? RefundRequestedAt { get; set; }
    public bool CanRequestRefund { get; set; }
    public RefundStatus? RefundStatus { get; set; }
    public string? RefundReason { get; set; }
    public string? RefundAdminNotes { get; set; }
}

public class RefundRequestViewModel
{
    public int PaymentId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string? CourseThumbnail { get; set; }
    public decimal AmountPaid { get; set; }

    [Required(ErrorMessage = "يرجى تحديد سبب طلب الاسترداد")]
    [Display(Name = "سبب الاسترداد")]
    public string Reason { get; set; } = string.Empty;

    [Display(Name = "وصف إضافي (اختياري)")]
    public string? Description { get; set; }
}


