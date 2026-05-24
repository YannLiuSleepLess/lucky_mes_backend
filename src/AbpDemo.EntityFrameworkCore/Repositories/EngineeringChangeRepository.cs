using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.Engineering.Changes.Aggregates;
using AbpDemo.Engineering.Changes.Repositories;
using AbpDemo.EntityFrameworkCore;
using AbpDemo.Enums;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.Domain.Repositories.EntityFrameworkCore;
using Volo.Abp.EntityFrameworkCore;

namespace AbpDemo.Repositories;

public class EngineeringChangeRepository : EfCoreRepository<AbpDemoDbContext, EngineeringChange, Guid>,
    IEngineeringChangeRepository
{
    public EngineeringChangeRepository(IDbContextProvider<AbpDemoDbContext> dbContextProvider)
        : base(dbContextProvider)
    {
    }

    public async Task<EngineeringChange> FindByEcnNoAsync(string ecnNo)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(e => e.EcnNo == ecnNo);
    }

    public async Task<List<EngineeringChange>> GetByStatusAsync(EcnStatus status)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.Where(e => e.Status == status).ToListAsync();
    }

    public async Task<List<EngineeringChange>> GetByProductAsync(Guid productId)
    {
        var dbSet = await GetDbSetAsync();
        var pidStr = productId.ToString();
        // 由于 AffectedProductIds 是 JSON 字符串，这里使用简单的包含查询
        // 在生产环境中，建议拆分为关联表以获得更好的性能和准确性
        return await dbSet.Where(e => e.AffectedProductIds != null && e.AffectedProductIds.Contains(pidStr))
            .ToListAsync();
    }
}