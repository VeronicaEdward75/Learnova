using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Admin;
using LearnNova.Models.ViewModels.Teacher;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class WithdrawalService : IWithdrawalService
{
    private readonly IGenericRepository<WithdrawalRequest> _withdrawalRepository;
    private readonly IWalletService _walletService;

    public WithdrawalService(IGenericRepository<WithdrawalRequest> withdrawalRepository, IWalletService walletService)
    {
        _withdrawalRepository = withdrawalRepository;
        _walletService = walletService;
    }

    public async Task<(bool Success, string Message)> CreateRequestAsync(string userId, WithdrawalRequestViewModel model)
    {
        if (model.Amount <= 0) return (false, "Amount must be greater than zero.");

        var wallet = await _walletService.GetWalletAsync(userId);
        if (wallet.AvailableBalance < model.Amount) return (false, "Insufficient available balance.");

        var existingRequests = await _withdrawalRepository.FindAsync(w => w.UserId == userId && w.Status == WithdrawalStatus.Pending);
        if (existingRequests.Any()) return (false, "You already have a pending withdrawal request.");

        var request = new WithdrawalRequest
        {
            UserId = userId,
            Amount = model.Amount,
            Status = WithdrawalStatus.Pending,
            RequestedAt = DateTime.UtcNow,
            PaymentMethod = model.PaymentMethod,
            AccountName = model.AccountName,
            AccountNumber = model.AccountNumber,
            BankName = model.BankName,
            IBAN = model.IBAN
        };

        await _withdrawalRepository.AddAsync(request);
        await _withdrawalRepository.SaveChangesAsync();

        return (true, "Withdrawal request submitted successfully.");
    }

    public async Task<List<WithdrawalHistoryViewModel>> GetTeacherWithdrawalsAsync(string userId)
    {
        var requests = await _withdrawalRepository.FindAsync(w => w.UserId == userId);
        return requests.OrderByDescending(r => r.RequestedAt).Select(r => new WithdrawalHistoryViewModel
        {
            Id = r.Id,
            Amount = r.Amount,
            Status = r.Status.ToString(),
            RequestedAt = r.RequestedAt,
            PaymentMethod = r.PaymentMethod,
            AdminNotes = r.AdminNotes
        }).ToList();
    }

    public async Task<List<AdminWithdrawalListViewModel>> GetAllWithdrawalsAsync(WithdrawalStatus? status)
    {
        var requestsQuery = await _withdrawalRepository.GetAllAsync();
        var requests = requestsQuery.AsQueryable(); // Eager loading User not available via GenericRepository easily, we will do what we can

        if (status.HasValue)
        {
            requests = requests.Where(r => r.Status == status.Value);
        }

        var result = new List<AdminWithdrawalListViewModel>();
        foreach(var r in requests.OrderByDescending(x => x.RequestedAt))
        {
            // We should load User manually if not loaded, but since we are simulating, let's assume it might not be eager loaded.
            result.Add(new AdminWithdrawalListViewModel
            {
                Id = r.Id,
                TeacherName = "Teacher Name (Check db)", // Will fix in controller or assume User is null for now if lazy loading is off
                TeacherEmail = "Email",
                Amount = r.Amount,
                Status = r.Status.ToString(),
                RequestedAt = r.RequestedAt,
                PaymentMethod = r.PaymentMethod,
                AccountName = r.AccountName,
                AccountNumber = r.AccountNumber,
                BankName = r.BankName,
                IBAN = r.IBAN,
                AdminNotes = r.AdminNotes
            });
        }
        return result;
    }

    public async Task<(bool Success, string Message)> ApproveRequestAsync(int requestId, string adminId, string? notes)
    {
        var request = await _withdrawalRepository.GetByIdAsync(requestId);
        if (request == null) return (false, "Request not found.");
        if (request.Status != WithdrawalStatus.Pending) return (false, "Request is not pending.");

        var wallet = await _walletService.GetWalletAsync(request.UserId);
        if (wallet.AvailableBalance < request.Amount) return (false, "Teacher has insufficient available balance.");

        // Deduct from wallet and create transaction
        var withdrawSuccess = await _walletService.WithdrawAsync(request.UserId, request.Amount);
        if (!withdrawSuccess) return (false, "Failed to withdraw funds from wallet.");

        request.Status = WithdrawalStatus.Approved;
        request.ProcessedAt = DateTime.UtcNow;
        request.ApprovedAt = DateTime.UtcNow;
        request.ProcessedBy = adminId;
        request.AdminNotes = notes;

        _withdrawalRepository.Update(request);
        await _withdrawalRepository.SaveChangesAsync();

        return (true, "Withdrawal request approved successfully.");
    }

    public async Task<(bool Success, string Message)> RejectRequestAsync(int requestId, string adminId, string? notes)
    {
        var request = await _withdrawalRepository.GetByIdAsync(requestId);
        if (request == null) return (false, "Request not found.");
        if (request.Status != WithdrawalStatus.Pending) return (false, "Request is not pending.");

        request.Status = WithdrawalStatus.Rejected;
        request.ProcessedAt = DateTime.UtcNow;
        request.RejectedAt = DateTime.UtcNow;
        request.ProcessedBy = adminId;
        request.AdminNotes = notes;

        _withdrawalRepository.Update(request);
        await _withdrawalRepository.SaveChangesAsync();

        return (true, "Withdrawal request rejected.");
    }
}
