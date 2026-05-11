using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.EntityFrameworkCore;
using AbpDemo.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpDemo.Engineering.Processes;

public class ProcessRouteRepository : EfCoreRepository<AbpDemoDbContext, ProcessRoute, Guid>,
    IProcessRouteRepository
{
    public ProcessRouteRepository(IDbContextProvider<AbpDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<ProcessRoute> GetWithStepsAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(r => r.Steps)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ProcessRoute> GetWithParametersAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(r => r.Parameters)
            .FirstOrDefaultAsync(r => r.Id == id);
    }

    public async Task<ProcessRoute> FindByCodeAsync(string routeCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(r => r.RouteCode == routeCode);
    }

    public async Task<List<ProcessRoute>> GetByProductAsync(Guid productId)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(r => r.ProductId == productId).ToListAsync();
    }

    public async Task<List<ProcessRoute>> GetByStatusAsync(ProcessRouteStatus status)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(r => r.Status == status).ToListAsync();
    }

    public async Task<bool> IsCodeUniqueAsync(string routeCode, Guid? excludeId = null)
    {
        var dbSet = await GetDbSetAsync();
        var query = dbSet.Where(r => r.RouteCode == routeCode);
        
        if (excludeId.HasValue)
        {
            query = query.Where(r => r.Id != excludeId.Value);
        }
        
        return !await query.AnyAsync();
    }
}
