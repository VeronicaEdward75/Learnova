using LearnNova.Data;
using LearnNova.Models.Entities;

namespace LearnNova.Repositories;

public class SystemSettingsRepository : GenericRepository<SystemSettings>, ISystemSettingsRepository
{
    public SystemSettingsRepository(ApplicationDbContext context) : base(context)
    {
    }
}
