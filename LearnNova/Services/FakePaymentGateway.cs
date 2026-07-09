using System;
using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public class FakePaymentGateway : IPaymentGateway
{
    private readonly Random _random = new Random();

    public async Task<(bool IsSuccess, string TransactionId, string GatewayResponse)> ProcessPaymentAsync(Payment payment)
    {
        // Simulate network delay 1-2 seconds
        int delay = _random.Next(1000, 2000);
        await Task.Delay(delay);

        // Simulate 90% success rate
        int chance = _random.Next(1, 101);
        if (chance <= 90)
        {
            string transactionId = $"fake_txn_{Guid.NewGuid().ToString("N").Substring(0, 10)}";
            return (true, transactionId, "Success");
        }
        else
        {
            return (false, string.Empty, "Insufficient funds or card declined");
        }
    }

    public async Task<bool> RefundPaymentAsync(Payment payment)
    {
        await Task.Delay(500);
        return true;
    }
}
