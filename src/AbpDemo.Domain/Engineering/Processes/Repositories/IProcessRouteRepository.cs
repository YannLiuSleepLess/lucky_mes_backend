using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.Enums;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.Engineering.Processes;

public interface IProcessRouteRepository : IRepository<ProcessRoute, Guid>
{
    Task<ProcessRoute> GetWithStepsAsync(Guid id); // 包含工序子实体
    Task<ProcessRoute> GetWithParametersAsync(Guid id); // 包含参数子实体
    Task<ProcessRoute> FindByCodeAsync(string routeCode);
    Task<List<ProcessRoute>> GetByProductAsync(Guid productId);
    Task<List<ProcessRoute>> GetByStatusAsync(ProcessRouteStatus status);
    Task<bool> IsCodeUniqueAsync(string routeCode, Guid? excludeId = null);
}