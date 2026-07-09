namespace LearnNova.Models.ViewModels.Student;

public class CheckoutSuccessViewModel
{
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
}
