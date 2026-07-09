using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface IPaymentService
{
    Task<Payment> CreatePaymentAsync(int courseId, string studentId);
    Task<bool> MarkSucceededAsync(int paymentId, string transactionId);
    Task<bool> MarkFailedAsync(int paymentId, string gatewayResponse);
    Task<bool> RefundAsync(int paymentId);
    Task<Payment?> GetPaymentAsync(int paymentId);
}
