using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.BasicData.WorkCenters.Dtos;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace AbpDemo.BasicData.WorkCenters;

public interface IWorkCenterAppService : IApplicationService
{
    Task<PagedResultDto<WorkCenterDto>> GetListAsync(GetWorkCenterListRequest input);
    Task<WorkCenterDto> GetAsync(Guid id);
    Task<WorkCenterDto> CreateAsync(CreateWorkCenterRequest input);
    Task<WorkCenterDto> UpdateAsync(Guid id, UpdateWorkCenterRequest input);
    Task DeleteAsync(Guid id);
    Task DeleteManyAsync(List<Guid> ids);
}