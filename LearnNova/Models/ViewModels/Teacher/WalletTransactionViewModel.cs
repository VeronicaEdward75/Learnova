using System;

namespace LearnNova.Models.ViewModels.Teacher;

public class WalletTransactionViewModel
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string CourseName { get; set; } = string.Empty;
    public string StudentName { get; set; } = string.Empty;
}
