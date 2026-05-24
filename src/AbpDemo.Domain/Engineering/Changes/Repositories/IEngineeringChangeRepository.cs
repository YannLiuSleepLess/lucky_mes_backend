using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.Engineering.Changes.Aggregates;
using AbpDemo.Enums;
using Volo.Abp.Domain.Repositories;

namespace AbpDemo.Engineering.Changes.Repositories;

public interface IEngineeringChangeRepository : IRepository<EngineeringChange, Guid>
{
    Task<EngineeringChange> FindByEcnNoAsync(string ecnNo);
    Task<List<EngineeringChange>> GetByStatusAsync(EcnStatus status);
    Task<List<EngineeringChange>> GetByProductAsync(Guid productId);
}