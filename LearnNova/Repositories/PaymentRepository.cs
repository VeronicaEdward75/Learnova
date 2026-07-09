using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class PaymentRepository : GenericRepository<Payment>, IPaymentRepository
{
    public PaymentRepository(ApplicationDbContext context) : base(context)
    {
    }
}
