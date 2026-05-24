using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.BasicData.Workshops.Aggregates;
using AbpDemo.BasicData.Workshops.Repositories;
using AbpDemo.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpDemo.BasicData.Workshops;

public class WorkshopRepository : EfCoreRepository<AbpDemoDbContext, Workshop, Guid>,
    IWorkshopRepository
{
    public WorkshopRepository(IDbContextProvider<AbpDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<Workshop> FindByCodeAsync(string workshopCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(w => w.WorkshopCode == workshopCode);
    }

    public async Task<List<Workshop>> GetActiveListAsync()
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string workshopCode, Guid? excludeId = null)
    {
        var dbSet = await GetDbSetAsync();
        return !await dbSet.AnyAsync(w =>
            w.WorkshopCode == workshopCode && (!excludeId.HasValue || w.Id != excludeId.Value));
    }
}