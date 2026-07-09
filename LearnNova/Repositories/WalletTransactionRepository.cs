using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(ApplicationDbContext context) : base(context)
    {
    }
}
