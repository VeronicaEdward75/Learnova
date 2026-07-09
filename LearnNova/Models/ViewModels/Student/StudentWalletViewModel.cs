using System.Collections.Generic;
using LearnNova.Models.ViewModels.Teacher; // Reusing WalletTransactionViewModel

namespace LearnNova.Models.ViewModels.Student;

public class StudentWalletViewModel
{
    public decimal AvailableBalance { get; set; }
    public List<WalletTransactionViewModel> Transactions { get; set; } = new List<WalletTransactionViewModel>();
}
