using System.Threading.Tasks;
using LearnNova.Models.Entities;

namespace LearnNova.Services;

public interface IInvoiceService
{
    Task<Invoice> CreateInvoiceAsync(int paymentId);
}
