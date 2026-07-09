using System;

namespace LearnNova.Models.Entities;

public class SystemSettings
{
    public int Id { get; set; }
    public decimal PlatformCommissionPercentage { get; set; }
    public decimal MinimumWithdrawal { get; set; }
    public decimal VatPercentage { get; set; }
    public string DefaultCurrency { get; set; } = "USD";
    public int RefundDays { get; set; } = 14;
    
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

