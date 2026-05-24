using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.BasicData.Workshops.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpDemo.BasicData.Workshops;

public interface IWorkshopAppService : IApplicationService
{
    Task<PagedResultDto<WorkshopDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    Task<WorkshopDto> GetAsync(Guid id);
    Task<WorkshopDto> CreateAsync(CreateWorkshopRequest input);
    Task<WorkshopDto> UpdateAsync(Guid id, UpdateWorkshopRequest input);
    Task DeleteAsync(Guid id);
    Task DeleteManyAsync(List<Guid> ids);
}