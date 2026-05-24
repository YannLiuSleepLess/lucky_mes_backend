using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.BasicData.WorkCenters.Aggregates;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.BasicData.WorkCenters.Repositories;

public interface IWorkCenterRepository : IRepository<WorkCenter, Guid>
{
    Task<WorkCenter> FindByCodeAsync(string workCenterCode);
    Task<List<WorkCenter>> GetByWorkshopAsync(Guid workshopId);
    Task<List<WorkCenter>> GetActiveListAsync();
    Task<bool> IsCodeUniqueAsync(string workCenterCode, Guid? excludeId = null);
}