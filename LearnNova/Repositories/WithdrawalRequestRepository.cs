using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class WithdrawalRequestRepository : GenericRepository<WithdrawalRequest>, IWithdrawalRequestRepository
{
    public WithdrawalRequestRepository(ApplicationDbContext context) : base(context)
    {
    }
}
