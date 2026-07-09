using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class WalletRepository : GenericRepository<Wallet>, IWalletRepository
{
    public WalletRepository(ApplicationDbContext context) : base(context)
    {
    }
}
