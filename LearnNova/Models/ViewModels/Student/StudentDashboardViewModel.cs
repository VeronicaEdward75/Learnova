using System.Collections.Generic;
using LearnNova.Models.Entities;
using LearnNova.Models.ViewModels.Teacher; // Reusing WalletTransactionViewModel

namespace LearnNova.Models.ViewModels.Student;

public class StudentDashboardViewModel
{
    public IEnumerable<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
    public IEnumerable<Payment> RecentPurchases { get; set; } = new List<Payment>();
    public IEnumerable<RefundRequest> RefundRequests { get; set; } = new List<RefundRequest>();
    public Wallet Wallet { get; set; } = null!;
    public IEnumerable<WalletTransactionViewModel> Transactions { get; set; } = new List<WalletTransactionViewModel>();
}
