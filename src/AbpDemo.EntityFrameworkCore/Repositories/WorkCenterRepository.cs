using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.BasicData.WorkCenters.Aggregates;
using AbpDemo.BasicData.WorkCenters.Repositories;
using AbpDemo.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpDemo.BasicData.WorkCenters;

public class WorkCenterRepository : EfCoreRepository<AbpDemoDbContext, WorkCenter, Guid>,
    IWorkCenterRepository
{
    public WorkCenterRepository(IDbContextProvider<AbpDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<WorkCenter> FindByCodeAsync(string workCenterCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(w => w.WorkCenterCode == workCenterCode);
    }

    public async Task<List<WorkCenter>> GetByWorkshopAsync(Guid workshopId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(w => w.WorkshopId == workshopId).ToListAsync();
    }

    public async Task<List<WorkCenter>> GetActiveListAsync()
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(w => w.IsActive).ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string workCenterCode, Guid? excludeId = null)
    {
        var dbSet = await GetDbSetAsync();
        return !await dbSet.AnyAsync(w =>
            w.WorkCenterCode == workCenterCode && (!excludeId.HasValue || w.Id != excludeId.Value));
    }
}