using System;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Models.Enums;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class PaymentService : IPaymentService
{
    private readonly IPaymentRepository _paymentRepository;

    public PaymentService(IPaymentRepository paymentRepository)
    {
        _paymentRepository = paymentRepository;
    }

    public async Task<Payment> CreatePaymentAsync(int courseId, string studentId)
    {
        var payment = new Payment
        {
            CourseId = courseId,
            StudentId = studentId,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
        await _paymentRepository.AddAsync(payment);
        await _paymentRepository.SaveChangesAsync();
        return payment;
    }

    public async Task<bool> MarkSucceededAsync(int paymentId, string transactionId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.Status = PaymentStatus.Succeeded;
        payment.TransactionId = transactionId;
        payment.CompletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> MarkFailedAsync(int paymentId, string gatewayResponse)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.Status = PaymentStatus.Failed;
        payment.GatewayResponse = gatewayResponse;
        payment.FailedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<bool> RefundAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) return false;

        payment.Status = PaymentStatus.Refunded;
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        _paymentRepository.Update(payment);
        await _paymentRepository.SaveChangesAsync();
        return true;
    }

    public async Task<Payment?> GetPaymentAsync(int paymentId)
    {
        return await _paymentRepository.GetByIdAsync(paymentId);
    }
}
