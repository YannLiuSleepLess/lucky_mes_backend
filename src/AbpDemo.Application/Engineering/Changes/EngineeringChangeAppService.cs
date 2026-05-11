using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using AbpDemo.Engineering.Changes.Dtos;
using AbpDemo.Enums;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.EventBus.Distributed;

namespace AbpDemo.Engineering.Changes;

public class EngineeringChangeAppService : AbpDemoAppService, IEngineeringChangeAppService
{
    private readonly IEngineeringChangeRepository _ecnRepository;
    private readonly IDistributedEventBus _distributedEventBus;

    public EngineeringChangeAppService(
        IEngineeringChangeRepository ecnRepository,
        IDistributedEventBus distributedEventBus)
    {
        _ecnRepository = ecnRepository;
        _distributedEventBus = distributedEventBus;
    }

    public async Task<PagedResultDto<EngineeringChangeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        var queryable = await _ecnRepository.GetQueryableAsync();
        var query = queryable
            .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "CreationTime DESC" : input.Sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var list = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<EngineeringChangeDto>(
            totalCount,
            ObjectMapper.Map<List<EngineeringChange>, List<EngineeringChangeDto>>(list)
        );
    }

    public async Task<EngineeringChangeDto> GetAsync(Guid id)
    {
        var entity = await _ecnRepository.GetAsync(id);
        return ObjectMapper.Map<EngineeringChange, EngineeringChangeDto>(entity);
    }

    public async Task<EngineeringChangeDto> CreateAsync(CreateEngineeringChangeRequest input)
    {
        // 1. 检查 ECN 编号唯一性
        if (await IsEcnNoUniqueAsync(input.EcnNo))
        {
            throw new UserFriendlyException("ECN 编号已存在");
        }

        var ecn = new EngineeringChange(
            GuidGenerator.Create(),
            input.EcnNo,
            input.Title,
            input.Type
        );

        ecn.SetDescription(input.Description);
        ecn.SetPriority(input.Priority);

        // 2. 设置关联 ID (这里简化处理，实际生产中可能需要领域服务校验这些 ID 是否真实存在)
        ecn.SetAffectedProducts(input.AffectedProductIds);
        ecn.SetAffectedBoms(input.AffectedBomVersionIds);
        ecn.SetAffectedRoutes(input.AffectedProcessRouteIds);

        await _ecnRepository.InsertAsync(ecn);
        return ObjectMapper.Map<EngineeringChange, EngineeringChangeDto>(ecn);
    }

    public async Task<EngineeringChangeDto> UpdateAsync(Guid id, UpdateEngineeringChangeRequest input)
    {
        var ecn = await _ecnRepository.GetAsync(id);

        // 检查 ECN 编号唯一性（排除自己）
        if (await IsEcnNoUniqueAsync(input.EcnNo, id))
        {
            throw new UserFriendlyException("ECN 编号已存在");
        }

        ecn.SetEcnNo(input.EcnNo);
        ecn.SetTitle(input.Title);
        ecn.SetDescription(input.Description);
        ecn.SetType(input.Type);
        ecn.SetPriority(input.Priority);
        ecn.SetAffectedProducts(input.AffectedProductIds);
        ecn.SetAffectedBoms(input.AffectedBomVersionIds);
        ecn.SetAffectedRoutes(input.AffectedProcessRouteIds);

        await _ecnRepository.UpdateAsync(ecn);
        return ObjectMapper.Map<EngineeringChange, EngineeringChangeDto>(ecn);
    }

    public async Task DeleteAsync(Guid id)
    {
        var ecn = await _ecnRepository.GetAsync(id);
        if (ecn.Status != EcnStatus.Draft && ecn.Status != EcnStatus.Cancelled)
        {
            throw new UserFriendlyException("只能删除草稿或已取消的ECN");
        }

        await _ecnRepository.DeleteAsync(id);
    }

    public async Task SubmitForReviewAsync(Guid id)
    {
        var ecn = await _ecnRepository.GetAsync(id);
        ecn.SubmitForReview();
        await _ecnRepository.UpdateAsync(ecn);
    }

    public async Task ApproveAsync(Guid id)
    {
        var ecn = await _ecnRepository.GetAsync(id);
        // 触发领域层的批准逻辑（状态流转）
        ecn.Approve(CurrentUser.Id ?? throw new UserFriendlyException("用户未登录"));
        await _ecnRepository.UpdateAsync(ecn);

        // TODO: 配置 RabbitMQ 后启用分布式事件发布
        // await _distributedEventBus.PublishAsync(
        //     new EcnApprovedEvent(ecn.Id, ecn.AffectedProductIds, CurrentUser.Id.Value)
        // );
    }

    public async Task ExecuteAsync(Guid id)
    {
        var ecn = await _ecnRepository.GetAsync(id);
        // 触发领域层的执行逻辑
        ecn.Execute();
        await _ecnRepository.UpdateAsync(ecn);

        // TODO: 配置 RabbitMQ 后启用分布式事件发布
        // await _distributedEventBus.PublishAsync(
        //     new EcnExecutedEvent(ecn.Id, ecn.AffectedBomVersionIds, ecn.AffectedProcessRouteIds)
        // );
    }

    private async Task<bool> IsEcnNoUniqueAsync(string ecnNo, Guid? excludeId = null)
    {
        var queryable = await _ecnRepository.GetQueryableAsync();
        var query = queryable.Where(e => e.EcnNo == ecnNo);

        if (excludeId.HasValue)
        {
            query = query.Where(e => e.Id != excludeId.Value);
        }

        return await AsyncExecuter.CountAsync(query) > 0;
    }
}