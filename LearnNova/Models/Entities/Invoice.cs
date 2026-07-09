using System;

namespace LearnNova.Models.Entities;

public class Invoice
{
    public int Id { get; set; }
    public string InvoiceNumber { get; set; } = string.Empty;
    
    public int PaymentId { get; set; }
    public Payment Payment { get; set; } = null!;
    
    public string StudentName { get; set; } = string.Empty;
    public string TeacherName { get; set; } = string.Empty;
    public string Currency { get; set; } = string.Empty;
    
    public decimal Subtotal { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal VAT { get; set; }
    public decimal Total { get; set; }
    
    public string? PaymentReference { get; set; }
    public DateTime IssueDate { get; set; } = DateTime.UtcNow;
}
