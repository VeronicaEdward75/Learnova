using System.Collections.Generic;

namespace LearnNova.Models.ViewModels.Teacher;

public class WalletViewModel
{
    public decimal PendingBalance { get; set; }
    public decimal AvailableBalance { get; set; }
    public decimal TotalWithdrawn { get; set; }
    public decimal TotalEarned { get; set; }
    public LearnNova.Models.ViewModels.Teacher.Filters.TeacherWalletFilterParameters Filters { get; set; } = new();
    public LearnNova.Models.ViewModels.PagedResult<WalletTransactionViewModel>? PagedTransactions { get; set; }
    // Kept for backward compatibility if needed, though we should prefer PagedTransactions
    public List<WalletTransactionViewModel> Transactions { get; set; } = new();
}
