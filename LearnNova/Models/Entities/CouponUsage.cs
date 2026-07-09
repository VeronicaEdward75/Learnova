using System;

namespace LearnNova.Models.Entities;

public class CouponUsage
{
    public int Id { get; set; }
    
    public int CouponId { get; set; }
    public Coupon Coupon { get; set; } = null!;
    
    public string StudentId { get; set; } = string.Empty;
    public ApplicationUser Student { get; set; } = null!;
    
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public DateTime UsedAt { get; set; } = DateTime.UtcNow;
}
