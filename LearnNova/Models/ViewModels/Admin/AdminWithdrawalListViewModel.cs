using System;

namespace LearnNova.Models.ViewModels.Admin;

public class AdminWithdrawalListViewModel
{
    public int Id { get; set; }
    public string TeacherName { get; set; } = string.Empty;
    public string TeacherEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime RequestedAt { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string AccountName { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string? BankName { get; set; }
    public string? IBAN { get; set; }
    public string? AdminNotes { get; set; }
}
