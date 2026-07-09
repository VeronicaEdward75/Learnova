using System.Collections.Generic;

namespace LearnNova.Models.ViewModels.Teacher;

public class WalletViewModel
{
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal TotalEarned { get; set; }
    public List<WalletTransactionViewModel> Transactions { get; set; } = new();
}
