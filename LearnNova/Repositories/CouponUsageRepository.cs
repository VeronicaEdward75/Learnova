using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class CouponUsageRepository : GenericRepository<CouponUsage>, ICouponUsageRepository
{
    public CouponUsageRepository(ApplicationDbContext context) : base(context)
    {
    }
}
