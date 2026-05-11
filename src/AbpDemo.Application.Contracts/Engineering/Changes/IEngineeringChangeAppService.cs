using System;
using System.Threading.Tasks;
using AbpDemo.Engineering.Changes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpDemo.Engineering.Changes;

public interface IEngineeringChangeAppService : IApplicationService
{
    Task<PagedResultDto<EngineeringChangeDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<EngineeringChangeDto> GetAsync(Guid id);
    Task<EngineeringChangeDto> CreateAsync(CreateEngineeringChangeRequest input);
    Task<EngineeringChangeDto> UpdateAsync(Guid id, UpdateEngineeringChangeRequest input);
    Task DeleteAsync(Guid id);
    Task SubmitForReviewAsync(Guid id);
    Task ApproveAsync(Guid id);
    Task ExecuteAsync(Guid id);
}