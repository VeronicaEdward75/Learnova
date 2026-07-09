using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class InvoiceRepository : GenericRepository<Invoice>, IInvoiceRepository
{
    public InvoiceRepository(ApplicationDbContext context) : base(context)
    {
    }
}
