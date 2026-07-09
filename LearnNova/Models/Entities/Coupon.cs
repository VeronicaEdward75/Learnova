using System;
using LearnNova.Models.Enums;

namespace LearnNova.Models.Entities;

public class Coupon
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public DiscountType DiscountType { get; set; }
    public decimal DiscountValue { get; set; }
    
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    
    public decimal? MinimumOrder { get; set; }
    public decimal? MaximumDiscount { get; set; }
    
    public int? UsageLimit { get; set; }
    
    public int? CourseId { get; set; }
    public Course? Course { get; set; }
    
    public string? TeacherId { get; set; }
    public ApplicationUser? Teacher { get; set; }
    
    public bool IsActive { get; set; } = true;
}
