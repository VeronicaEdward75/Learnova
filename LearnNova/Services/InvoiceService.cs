using System;
using System.Threading.Tasks;
using LearnNova.Models.Entities;
using LearnNova.Repositories;

namespace LearnNova.Services;

public class InvoiceService : IInvoiceService
{
    private readonly IInvoiceRepository _invoiceRepository;
    private readonly IPaymentRepository _paymentRepository;

    public InvoiceService(IInvoiceRepository invoiceRepository, IPaymentRepository paymentRepository)
    {
        _invoiceRepository = invoiceRepository;
        _paymentRepository = paymentRepository;
    }

    public async Task<Invoice> CreateInvoiceAsync(int paymentId)
    {
        var payment = await _paymentRepository.GetByIdAsync(paymentId);
        if (payment == null) throw new Exception("Payment not found");

        var invoice = new Invoice
        {
            PaymentId = payment.Id,
            InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}",
            StudentName = payment.Student?.FullName ?? "Student",
            TeacherName = payment.Course?.Teacher?.FullName ?? "Teacher",
            Currency = payment.Currency,
            Subtotal = payment.TeacherAmount,
            PlatformFee = payment.PlatformFee,
            VAT = 0, // VAT can be extracted from system settings via payment properties
            Total = payment.StudentPaid,
            PaymentReference = payment.PaymentReference,
            IssueDate = DateTime.UtcNow
        };

        await _invoiceRepository.AddAsync(invoice);
        await _invoiceRepository.SaveChangesAsync();
        return invoice;
    }
}
