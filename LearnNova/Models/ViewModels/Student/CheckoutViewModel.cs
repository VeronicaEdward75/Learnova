using LearnNova.Models.Entities;

namespace LearnNova.Models.ViewModels.Student;

public class CheckoutViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string CourseImage { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;

    public decimal Price { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal VatAmount { get; set; }
    public decimal Total { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal FinalTotal { get; set; }
    public string? CouponCode { get; set; }
    public string? CouponError { get; set; }
    public string Currency { get; set; } = "EGP";

    public string PaymentMethod { get; set; } = "CreditCard";
}

