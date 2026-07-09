using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class WalletTransaction
{
    public int Id { get; set; }
    
    public int WalletId { get; set; }
    public Wallet Wallet { get; set; } = null!;

    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }

    public decimal Amount { get; set; }
    public TransactionType Type { get; set; }
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
