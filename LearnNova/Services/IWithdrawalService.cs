using System.Collections.Generic;
using System.Threading.Tasks;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Admin;
using LearnNova.Models.ViewModels.Teacher;

namespace LearnNova.Services;

public interface IWithdrawalService
{
    Task<(bool Success, string Message)> CreateRequestAsync(string userId, WithdrawalRequestViewModel model);
    Task<List<WithdrawalHistoryViewModel>> GetTeacherWithdrawalsAsync(string userId);
    Task<List<AdminWithdrawalListViewModel>> GetAllWithdrawalsAsync(WithdrawalStatus? status);
    Task<(bool Success, string Message)> ApproveRequestAsync(int requestId, string adminId, string? notes);
    Task<(bool Success, string Message)> RejectRequestAsync(int requestId, string adminId, string? notes);
}
