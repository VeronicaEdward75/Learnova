using System.ComponentModel.DataAnnotations;

namespace LearnNova.Models.ViewModels.Teacher;

public class WithdrawalRequestViewModel
{
    [Required(ErrorMessage = "المبلغ مطلوب")]
    [Range(1, double.MaxValue, ErrorMessage = "يجب أن يكون المبلغ أكبر من صفر")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "طريقة الدفع مطلوبة")]
    public string PaymentMethod { get; set; } = string.Empty;

    [Required(ErrorMessage = "اسم الحساب مطلوب")]
    public string AccountName { get; set; } = string.Empty;

    [Required(ErrorMessage = "رقم الحساب أو المحفظة مطلوب")]
    public string AccountNumber { get; set; } = string.Empty;

    public string? BankName { get; set; }
    
    public string? IBAN { get; set; }

    // Display fields
    public decimal AvailableBalance { get; set; }
    public decimal MinimumWithdrawal { get; set; }
    public decimal MaximumWithdrawal { get; set; }
    public bool HasPendingRequest { get; set; }
}
