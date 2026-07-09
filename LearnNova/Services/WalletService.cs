using System;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;
using System.Collections.Generic;

namespace LearnNova.Services;

public class WalletService : IWalletService
{
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _walletTransactionRepository;
    private readonly IPaymentRepository _paymentRepository;

    public WalletService(IWalletRepository walletRepository, IWalletTransactionRepository walletTransactionRepository, IPaymentRepository paymentRepository)
    {
        _walletRepository = walletRepository;
        _walletTransactionRepository = walletTransactionRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Wallet> GetWalletAsync(string userId)
    {
        var wallet = (await _walletRepository.GetAllAsync()).FirstOrDefault(w => w.UserId == userId);
        if (wallet == null)
        {
            wallet = new Wallet { UserId = userId, AvailableBalance = 0, PendingBalance = 0, TotalEarned = 0, TotalWithdrawn = 0 };
            await _walletRepository.AddAsync(wallet);
            await _walletRepository.SaveChangesAsync();
        }
        return wallet;
    }

    public async Task AddPendingBalanceAsync(string userId, decimal amount)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        var wallet = await GetWalletAsync(userId);
        wallet.PendingBalance += amount;
        wallet.TotalEarned += amount; // We count pending in TotalEarned for display
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();
    }

    public async Task CreateTransactionAsync(int walletId, decimal amount, TransactionType type, string description, int? paymentId = null)
    {
        var wt = new WalletTransaction
        {
            WalletId = walletId,
            Amount = amount,
            Type = type,
            Description = description,
            PaymentId = paymentId,
            CreatedAt = DateTime.UtcNow
        };
        await _walletTransactionRepository.AddAsync(wt);
        await _walletTransactionRepository.SaveChangesAsync();
    }

    public async Task<List<WalletTransactionViewModel>> GetTransactionsAsync(int walletId)
    {
        var transactions = await _walletTransactionRepository.FindAsync(t => t.WalletId == walletId);
        
        var vms = new List<WalletTransactionViewModel>();
        foreach(var t in transactions.OrderByDescending(x => x.CreatedAt))
        {
            var p = t.PaymentId.HasValue ? await _paymentRepository.GetByIdAsync(t.PaymentId.Value) : null;
            vms.Add(new WalletTransactionViewModel
            {
                Id = t.Id,
                Date = t.CreatedAt,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description ?? string.Empty,
                Status = "Completed",
                CourseName = p?.Course?.Title ?? "N/A",
                StudentName = p?.Student?.FullName ?? "N/A"
            });
        }
        return vms;
    }

    public async Task<LearnNova.Models.ViewModels.PagedResult<WalletTransactionViewModel>> GetTransactionsPagedAsync(int walletId, LearnNova.Models.ViewModels.Student.Filters.WalletFilterParameters filters)
    {
        var query = _walletTransactionRepository.GetQueryable()
            .Where(t => t.WalletId == walletId);

        if (filters.StartDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= filters.StartDate.Value);
        }

        if (filters.EndDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= filters.EndDate.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.Type))
        {
            if (Enum.TryParse<TransactionType>(filters.Type, true, out var typeEnum))
            {
                query = query.Where(t => t.Type == typeEnum);
            }
        }

        query = filters.SortBy switch
        {
            "oldest" => query.OrderBy(t => t.CreatedAt),
            "amount_desc" => query.OrderByDescending(t => t.Amount),
            "amount_asc" => query.OrderBy(t => t.Amount),
            _ => query.OrderByDescending(t => t.CreatedAt)
        };

        var totalCount = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.CountAsync(query);
        
        int pageNumber = filters.PageNumber > 0 ? filters.PageNumber : 1;
        int pageSize = filters.PageSize > 0 ? filters.PageSize : 20;

        var pagedTransactions = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.ToListAsync(query.Skip((pageNumber - 1) * pageSize).Take(pageSize));

        var vms = new List<WalletTransactionViewModel>();
        foreach(var t in pagedTransactions)
        {
            var p = t.PaymentId.HasValue ? await _paymentRepository.GetByIdAsync(t.PaymentId.Value) : null;
            // Ensure Payment has course loaded, we might need a better query but this matches existing logic
            if (p != null)
            {
                p = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
                    Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(
                        Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.Include(_paymentRepository.GetQueryable(), x => x.Course), 
                        x => x.Student), 
                    x => x.Id == p.Id);
            }

            vms.Add(new WalletTransactionViewModel
            {
                Id = t.Id,
                Date = t.CreatedAt,
                Amount = t.Amount,
                Type = t.Type.ToString(),
                Description = t.Description ?? string.Empty,
                Status = "Completed",
                CourseName = p?.Course?.Title ?? "N/A",
                StudentName = p?.Student?.FullName ?? "N/A"
            });
        }

        return new LearnNova.Models.ViewModels.PagedResult<WalletTransactionViewModel>
        {
            Items = vms,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<TeacherRevenueSummaryViewModel> GetRevenueSummaryAsync(string userId)
    {
        var wallet = await GetWalletAsync(userId);
        var thirtyDaysAgo = DateTime.UtcNow.AddDays(-30);
        
        var transactions = await _walletTransactionRepository.FindAsync(t => t.WalletId == wallet.Id && t.Type == TransactionType.Sale);
        var monthlyRevenue = transactions.Where(t => t.CreatedAt >= thirtyDaysAgo).Sum(t => t.Amount);

        var allPayments = await _paymentRepository.FindAsync(p => p.Status == PaymentStatus.Succeeded && p.Course != null && p.Course.TeacherId == userId);

        return new TeacherRevenueSummaryViewModel
        {
            PendingBalance = wallet.PendingBalance,
            AvailableBalance = wallet.AvailableBalance,
            TotalEarnings = wallet.TotalEarned,
            MonthlyRevenue = monthlyRevenue,
            TotalCoursesSold = allPayments.Count()
        };
    }

    public async Task<(decimal PlatformRevenue, decimal TeacherEarnings, decimal PendingRevenue)> GetPlatformRevenueStatsAsync()
    {
        var allPayments = await _paymentRepository.FindAsync(p => p.Status == PaymentStatus.Succeeded);
        
        decimal platformRevenue = allPayments.Sum(p => p.PlatformFee);
        decimal teacherEarnings = allPayments.Sum(p => p.TeacherAmount);
        
        var pendingPayments = await _paymentRepository.FindAsync(p => p.Status == PaymentStatus.Pending);
        decimal pendingRevenue = pendingPayments.Sum(p => p.PlatformFee);

        return (platformRevenue, teacherEarnings, pendingRevenue);
    }

    // Legacy methods from 13.1 interface
    public async Task<bool> CreditPendingAsync(string userId, decimal amount, int paymentId)
    {
        await AddPendingBalanceAsync(userId, amount);
        var wallet = await GetWalletAsync(userId);
        await CreateTransactionAsync(wallet.Id, amount, TransactionType.Sale, "Sale Credited", paymentId);
        return true;
    }

    public async Task<bool> CreditAvailableBalanceAsync(string userId, decimal amount, int? paymentId = null)
    {
        if (amount < 0) throw new ArgumentException("Amount cannot be negative");
        var wallet = await GetWalletAsync(userId);
        wallet.AvailableBalance += amount;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();

        await CreateTransactionAsync(wallet.Id, amount, TransactionType.Refund, "استرداد مدفوعات الدورة", paymentId);
        return true;
    }

    public async Task<bool> MovePendingToAvailableAsync(string userId, decimal amount)
    {
        var wallet = await GetWalletAsync(userId);
        if (wallet.PendingBalance < amount) return false;
        
        wallet.PendingBalance -= amount;
        wallet.AvailableBalance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> WithdrawAsync(string userId, decimal amount)
    {
        var wallet = await GetWalletAsync(userId);
        if (wallet.AvailableBalance < amount) return false;

        wallet.AvailableBalance -= amount;
        wallet.TotalWithdrawn += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();

        await CreateTransactionAsync(wallet.Id, amount, TransactionType.Withdrawal, "Funds Withdrawn");
        return true;
    }

    public async Task<bool> PayWithWalletAsync(string userId, decimal amount, int paymentId)
    {
        var wallet = await GetWalletAsync(userId);
        if (wallet.AvailableBalance < amount) return false;

        wallet.AvailableBalance -= amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();

        await CreateTransactionAsync(wallet.Id, amount, TransactionType.WalletPurchase, "دفع تكلفة الكورس من المحفظة", paymentId);
        return true;
    }

    public async Task<bool> CreditWalletRefundAsync(string userId, decimal amount, int paymentId)
    {
        if (amount <= 0) return false;
        var wallet = await GetWalletAsync(userId);
        wallet.AvailableBalance += amount;
        wallet.UpdatedAt = DateTime.UtcNow;
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();

        await CreateTransactionAsync(wallet.Id, amount, TransactionType.WalletRefund, "استرداد مدفوعات المحفظة", paymentId);
        return true;
    }

    public async Task<bool> ProcessRefundDeductionAsync(string userId, decimal amount)
    {
        var wallet = await GetWalletAsync(userId);
        if (wallet.PendingBalance >= amount)
        {
            wallet.PendingBalance -= amount;
        }
        else
        {
            var remainder = amount - wallet.PendingBalance;
            wallet.PendingBalance = 0;
            wallet.AvailableBalance -= remainder;
        }
        _walletRepository.Update(wallet);
        await _walletRepository.SaveChangesAsync();
        return true;
    }
}
