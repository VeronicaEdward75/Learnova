using System.Collections.Generic;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IWalletService
{
    Task<Wallet> GetWalletAsync(string userId);
    Task AddPendingBalanceAsync(string userId, decimal amount);
    Task CreateTransactionAsync(int walletId, decimal amount, TransactionType type, string description, int? paymentId = null);
    Task<List<WalletTransactionViewModel>> GetTransactionsAsync(int walletId);
    Task<TeacherRevenueSummaryViewModel> GetRevenueSummaryAsync(string userId);
    Task<(decimal PlatformRevenue, decimal TeacherEarnings, decimal PendingRevenue)> GetPlatformRevenueStatsAsync();
    
    Task<bool> CreditPendingAsync(string userId, decimal amount, int paymentId);
    Task<bool> CreditAvailableBalanceAsync(string userId, decimal amount, int? paymentId = null);
    Task<bool> MovePendingToAvailableAsync(string userId, decimal amount);
    Task<bool> WithdrawAsync(string userId, decimal amount);
    Task<bool> ProcessRefundDeductionAsync(string userId, decimal amount);
}

