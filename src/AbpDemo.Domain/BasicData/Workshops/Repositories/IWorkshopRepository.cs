using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.BasicData.Workshops.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.BasicData.Workshops.Repositories;

public interface IWorkshopRepository : IRepository<Workshop, Guid>
{
    Task<Workshop> FindByCodeAsync(string workshopCode);
    Task<List<Workshop>> GetActiveListAsync();
    Task<bool> IsCodeUniqueAsync(string workshopCode, Guid? excludeId = null);
}