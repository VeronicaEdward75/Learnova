using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.ViewModels.Admin;

public class AdminFinanceFilterParameters
{
    public string DateFilter { get; set; } = "This Month";
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string? SearchTerm { get; set; }
    public string? TeacherId { get; set; }
    public int? CourseId { get; set; }
    public string? Subject { get; set; }
    public string? Stage { get; set; }
    public string? PaymentMethod { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public RefundStatus? RefundStatus { get; set; }
}
