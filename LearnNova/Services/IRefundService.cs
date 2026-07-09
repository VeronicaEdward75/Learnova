using System.Collections.Generic;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Models.ViewModels.Student;
using LearnNova.Models.ViewModels.Admin;

namespace LearnNova.Services;

public interface IRefundService
{
    Task<(bool Success, string Message)> CanRequestRefundAsync(int paymentId, string studentId);
    Task<(bool Success, string Message)> RequestRefundAsync(int paymentId, string studentId, string reason, string? description);
    Task<(bool Success, string Message)> ApproveRefundAsync(int refundId, string? adminNotes);
    Task<(bool Success, string Message)> RejectRefundAsync(int refundId, string? adminNotes);
    
    Task<List<OrderHistoryViewModel>> GetStudentOrdersAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<OrderHistoryViewModel>> GetStudentOrdersPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.OrderFilterParameters filters);
    Task<List<StudentRefundViewModel>> GetStudentRefundRequestsAsync(string studentId);
    Task<LearnNova.Models.ViewModels.PagedResult<StudentRefundViewModel>> GetStudentRefundRequestsPagedAsync(string studentId, LearnNova.Models.ViewModels.Student.Filters.RefundFilterParameters filters);
    Task<OrderDetailsViewModel?> GetOrderDetailsAsync(int paymentId, string studentId);
    Task<List<AdminRefundViewModel>> GetAllRefundsAsync(RefundStatus? statusFilter = null);
}
