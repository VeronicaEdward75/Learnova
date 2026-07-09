using System;
using System.Collections.Generic;
using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;

    public int CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public decimal StudentPaid { get; set; }
    public decimal TeacherAmount { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal GatewayFee { get; set; }
    public string Currency { get; set; } = string.Empty;

    public string Gateway { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public string? TransactionId { get; set; }
    public string? PaymentReference { get; set; }
    public string? GatewayResponse { get; set; }

    public PaymentStatus Status { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public DateTime? RefundedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Invoice? Invoice { get; set; }
    public RefundRequest? RefundRequest { get; set; }
    public ICollection<WalletTransaction> WalletTransactions { get; set; } = new List<WalletTransaction>();
}

