using System;
using System.Threading.Tasks;
using AbpDemo.Engineering.Processes.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpDemo.Engineering.Processes;

public interface IProcessRouteAppService : IApplicationService
{
    Task<PagedResultDto<ProcessRouteDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<ProcessRouteDto> GetAsync(Guid id);
    Task<ProcessRouteDto> CreateAsync(CreateProcessRouteRequest input);
    Task<ProcessRouteDto> UpdateAsync(Guid id, UpdateProcessRouteRequest input);
    Task DeleteAsync(Guid id);
    Task PublishAsync(Guid id);
}