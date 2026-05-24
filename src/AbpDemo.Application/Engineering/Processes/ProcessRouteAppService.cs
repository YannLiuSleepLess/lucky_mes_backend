using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpDemo.Engineering.Processes.Dtos;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Processes;

public class ProcessRouteAppService(
    IProcessRouteRepository processRouteRepository
) : AbpDemoAppService, IProcessRouteAppService
{
    public async Task<PagedResultDto<ProcessRouteDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await processRouteRepository.GetQueryableAsync();
        
        // 使用投影查询，只获取需要的字段和步骤数量，不加载完整的 Steps 集合
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount)
            .Select(r => new ProcessRouteDto
            {
                Id = r.Id,
                RouteCode = r.RouteCode,
                RouteName = r.RouteName,
                ProductId = r.ProductId,
                Version = r.Version,
                Status = r.Status,
                StepCount = r.Steps.Count
            });

        var list = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<ProcessRouteDto>(
            totalCount,
            list
        );
    }

    public async Task<ProcessRouteDto> GetAsync(Guid id)
    {
        var entity = await processRouteRepository.GetWithStepsAsync(id);
        if (entity == null)
        {
            throw new EntityNotFoundException($"工艺路线不存在，ID: {id}");
        }

        return ObjectMapper.Map<ProcessRoute, ProcessRouteDto>(entity);
    }

    public async Task<ProcessRouteDto> CreateAsync(CreateProcessRouteRequest input)
    {
        var route = new ProcessRoute(
            GuidGenerator.Create(),
            input.RouteCode,
            input.RouteName,
            input.ProductId
        );

        // 添加工序步骤
        foreach (var stepInput in input.Steps)
        {
            route.AddStep(
                stepInput.StepNo,
                stepInput.StepName,
                stepInput.StandardTime,
                stepInput.IsKeyProcess
            );
        }

        await processRouteRepository.InsertAsync(route);
        return ObjectMapper.Map<ProcessRoute, ProcessRouteDto>(route);
    }

    public async Task<ProcessRouteDto> UpdateAsync(Guid id, UpdateProcessRouteRequest input)
    {
        var route = await processRouteRepository.GetAsync(id);

        route.SetRouteCode(input.RouteCode);
        route.SetRouteName(input.RouteName);
        route.SetProductId(input.ProductId);

        // 清空旧步骤，重新添加
        route.ClearSteps();
        foreach (var stepInput in input.Steps)
        {
            route.AddStep(
                stepInput.StepNo,
                stepInput.StepName,
                stepInput.StandardTime,
                stepInput.IsKeyProcess
            );
        }

        await processRouteRepository.UpdateAsync(route);
        return ObjectMapper.Map<ProcessRoute, ProcessRouteDto>(route);
    }

    public async Task DeleteAsync(Guid id)
    {
        await processRouteRepository.DeleteAsync(id);
    }

    public async Task PublishAsync(Guid id)
    {
        var route = await processRouteRepository.GetAsync(id);
        route.Publish(CurrentUser.Id ?? throw new UserFriendlyException("用户未登录"));
        await processRouteRepository.UpdateAsync(route);
    }
}