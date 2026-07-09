using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class CouponRepository : GenericRepository<Coupon>, ICouponRepository
{
    public CouponRepository(ApplicationDbContext context) : base(context)
    {
    }
}
