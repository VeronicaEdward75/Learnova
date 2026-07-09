using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface IPaymentGateway
{
    Task<(bool IsSuccess, string TransactionId, string GatewayResponse)> ProcessPaymentAsync(Payment payment);
    Task<bool> RefundPaymentAsync(Payment payment);
}
