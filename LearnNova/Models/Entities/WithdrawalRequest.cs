using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class WithdrawalRequest
{
    public int Id { get; set; }

    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;

    public decimal Amount { get; set; }
    public WithdrawalStatus Status { get; set; }

    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? RejectedAt { get; set; }

    public string? ProcessedBy { get; set; }
    public string? AdminNotes { get; set; }

    // Bank Details
    public string PaymentMethod { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? IBAN { get; set; }
}
