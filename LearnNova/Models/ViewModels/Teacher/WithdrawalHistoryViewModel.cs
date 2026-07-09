using System;

namespace LearnNova.Models.ViewModels.Teacher;

public class WithdrawalHistoryViewModel
{
    public int Id { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string? AdminNotes { get; set; }
}
