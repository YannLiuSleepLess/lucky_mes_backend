using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpDemo.BasicData.Workshops.Aggregates;
using AbpDemo.BasicData.Workshops.Dtos;
using AbpDemo.BasicData.Workshops.Repositories;
using Volo.Abp;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.BasicData.Workshops;

public class WorkshopAppService : AbpDemoAppService, IWorkshopAppService
{
    private readonly IWorkshopRepository _workshopRepository;

    public WorkshopAppService(IWorkshopRepository workshopRepository)
    {
        _workshopRepository = workshopRepository;
    }

    public async Task<PagedResultDto<WorkshopDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _workshopRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var workshops = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<WorkshopDto>(
            totalCount,
            ObjectMapper.Map<List<Workshop>, List<WorkshopDto>>(workshops)
        );
    }

    public async Task<WorkshopDto> GetAsync(Guid id)
    {
        var workshop = await _workshopRepository.GetAsync(id);
        return ObjectMapper.Map<Workshop, WorkshopDto>(workshop);
    }

    public async Task<WorkshopDto> CreateAsync(CreateWorkshopRequest input)
    {
        if (!await _workshopRepository.IsCodeUniqueAsync(input.WorkshopCode))
            throw new UserFriendlyException("车间编码已存在");

        var workshop = new Workshop(
            GuidGenerator.Create(),
            input.WorkshopCode,
            input.WorkshopName
        );

        workshop.SetLocation(input.Location);
        workshop.SetManager(input.ManagerId);

        await _workshopRepository.InsertAsync(workshop);
        return ObjectMapper.Map<Workshop, WorkshopDto>(workshop);
    }

    public async Task<WorkshopDto> UpdateAsync(Guid id, UpdateWorkshopRequest input)
    {
        var workshop = await _workshopRepository.GetAsync(id);

        workshop.SetWorkshopName(input.WorkshopName);
        workshop.SetLocation(input.Location);
        workshop.SetManager(input.ManagerId);
        workshop.SetActive(input.IsActive);

        await _workshopRepository.UpdateAsync(workshop);
        return ObjectMapper.Map<Workshop, WorkshopDto>(workshop);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _workshopRepository.DeleteAsync(id);
    }

    public async Task DeleteManyAsync(List<Guid> ids)
    {
        foreach (var id in ids)
        {
            await _workshopRepository.DeleteAsync(id);
        }
    }
}