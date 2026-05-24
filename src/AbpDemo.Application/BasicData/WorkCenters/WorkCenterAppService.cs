using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpDemo.BasicData.WorkCenters.Aggregates;
using AbpDemo.BasicData.WorkCenters.Dtos;
using AbpDemo.BasicData.WorkCenters.Repositories;
using AbpDemo.BasicData.Workshops.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.BasicData.WorkCenters;

public class WorkCenterAppService : AbpDemoAppService, IWorkCenterAppService
{
    private readonly IWorkCenterRepository _workCenterRepository;
    private readonly IWorkshopRepository _workshopRepository;

    public WorkCenterAppService(
        IWorkCenterRepository workCenterRepository,
        IWorkshopRepository workshopRepository)
    {
        _workCenterRepository = workCenterRepository;
        _workshopRepository = workshopRepository;
    }

    public async Task<PagedResultDto<WorkCenterDto>> GetListAsync(GetWorkCenterListRequest input)
    {
        var queryable = await _workCenterRepository.GetQueryableAsync();

        if (input.WorkshopId.HasValue)
        {
            queryable = queryable.Where(w => w.WorkshopId == input.WorkshopId.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var workCenters = await AsyncExecuter.ToListAsync(query);

        var dtos = new List<WorkCenterDto>();
        foreach (var wc in workCenters)
        {
            var dto = ObjectMapper.Map<WorkCenter, WorkCenterDto>(wc);
            var workshop = await _workshopRepository.FindAsync(wc.WorkshopId);
            dto.WorkshopName = workshop?.WorkshopName;
            dtos.Add(dto);
        }

        return new PagedResultDto<WorkCenterDto>(totalCount, dtos);
    }

    public async Task<WorkCenterDto> GetAsync(Guid id)
    {
        var workCenter = await _workCenterRepository.GetAsync(id);
        var dto = ObjectMapper.Map<WorkCenter, WorkCenterDto>(workCenter);

        var workshop = await _workshopRepository.FindAsync(workCenter.WorkshopId);
        dto.WorkshopName = workshop?.WorkshopName;

        return dto;
    }

    public async Task<WorkCenterDto> CreateAsync(CreateWorkCenterRequest input)
    {
        if (!await _workCenterRepository.IsCodeUniqueAsync(input.WorkCenterCode))
            throw new UserFriendlyException("工作中心编码已存在");

        var workshop = await _workshopRepository.FindAsync(input.WorkshopId);
        if (workshop == null)
            throw new UserFriendlyException("所属车间不存在");

        var workCenter = new WorkCenter(
            GuidGenerator.Create(),
            input.WorkCenterCode,
            input.WorkCenterName,
            input.WorkshopId
        );

        workCenter.SetCapacity(input.Capacity);
        workCenter.SetShiftCount(input.ShiftCount);

        await _workCenterRepository.InsertAsync(workCenter);

        var dto = ObjectMapper.Map<WorkCenter, WorkCenterDto>(workCenter);
        dto.WorkshopName = workshop.WorkshopName;
        return dto;
    }

    public async Task<WorkCenterDto> UpdateAsync(Guid id, UpdateWorkCenterRequest input)
    {
        var workCenter = await _workCenterRepository.GetAsync(id);

        var workshop = await _workshopRepository.FindAsync(input.WorkshopId);
        if (workshop == null)
            throw new UserFriendlyException("所属车间不存在");

        workCenter.SetWorkCenterName(input.WorkCenterName);
        workCenter.SetWorkshopId(input.WorkshopId);
        workCenter.SetCapacity(input.Capacity);
        workCenter.SetShiftCount(input.ShiftCount);
        workCenter.SetActive(input.IsActive);

        await _workCenterRepository.UpdateAsync(workCenter);

        var dto = ObjectMapper.Map<WorkCenter, WorkCenterDto>(workCenter);
        dto.WorkshopName = workshop.WorkshopName;
        return dto;
    }

    public async Task DeleteAsync(Guid id)
    {
        await _workCenterRepository.DeleteAsync(id);
    }

    public async Task DeleteManyAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await _workCenterRepository.DeleteAsync(id);
        }
    }
}