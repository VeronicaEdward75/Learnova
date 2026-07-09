using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Admin;

public class AdminRefundViewModel
{
    public int RefundId { get; set; }
    public int PaymentId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string CourseTitle { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    
    public decimal Amount { get; set; }
    public decimal TeacherLoss { get; set; }
    public decimal PlatformLoss { get; set; }
    public decimal StudentRefund { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? AdminNotes { get; set; }
    
    public RefundStatus Status { get; set; }
    public DateTime RequestedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
