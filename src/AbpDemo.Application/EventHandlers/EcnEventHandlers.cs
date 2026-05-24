using System;
using System.Threading.Tasks;
using AbpDemo.Engineering.Changes.Events;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus.Distributed;

namespace AbpDemo.EventHandlers;

public class EcnApprovedEventHandler : IDistributedEventHandler<EcnApprovedEvent>, ITransientDependency
{
    public async Task HandleEventAsync(EcnApprovedEvent eventData)
    {
        // 1. 发送通知给相关人员
        Console.WriteLine($"ECN {eventData.EcnId} 已被批准，批准人: {eventData.ApprovedBy}");

        // 2. 触发后续流程（如：准备执行环境）
        await Task.CompletedTask;
    }
}

public class EcnExecutedEventHandler : IDistributedEventHandler<EcnExecutedEvent>, ITransientDependency
{
    public async Task HandleEventAsync(EcnExecutedEvent eventData)
    {
        // 1. 通知 ERP/PDM 系统同步变更
        Console.WriteLine($"ECN {eventData.EcnId} 已执行完成");

        // 2. 更新相关缓存
        await Task.CompletedTask;
    }
}