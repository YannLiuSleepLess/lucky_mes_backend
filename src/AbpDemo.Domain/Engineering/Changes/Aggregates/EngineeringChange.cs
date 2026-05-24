using System;
using System.Collections.Generic;
using System.Text.Json;
using AbpDemo.Enums;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpDemo.Engineering.Changes.Aggregates;

/// <summary>
/// 工程变更单 (ECN) 聚合根
/// </summary>
public class EngineeringChange : FullAuditedAggregateRoot<Guid>
{
    public string EcnNo { get; private set; }
    public string Title { get; private set; }
    public string Description { get; private set; }
    public ChangeType Type { get; private set; }
    public EcnStatus Status { get; private set; }
    public Priority Priority { get; private set; }

    // 关联的受影响对象ID列表（实际项目中通常会拆分为子实体表）
    public string AffectedProductIds { get; private set; } // JSON存储
    public string AffectedBomVersionIds { get; private set; } // JSON存储
    public string AffectedProcessRouteIds { get; private set; } // JSON存储

    public Guid? ReviewedBy { get; private set; }
    public DateTime? ReviewedAt { get; private set; }
    public Guid? ApprovedBy { get; private set; }
    public DateTime? ApprovedAt { get; private set; }
    public DateTime? ExecutedAt { get; private set; }

    protected EngineeringChange()
    {
    }

    public EngineeringChange(Guid id, string ecnNo, string title, ChangeType type) : base(id)
    {
        EcnNo = ecnNo ?? throw new ArgumentNullException(nameof(ecnNo));
        Title = title ?? throw new ArgumentNullException(nameof(title));
        Type = type;
        Status = EcnStatus.Draft;
        Priority = Priority.Medium;
    }

    public void SubmitForReview()
    {
        if (Status != EcnStatus.Draft)
            throw new UserFriendlyException("只有草稿状态的ECN才能提交审核");
        Status = EcnStatus.PendingReview;
    }

    public void Approve(Guid approvedBy)
    {
        if (Status != EcnStatus.PendingReview && !(Priority == Priority.Urgent && Status == EcnStatus.Draft))
            throw new UserFriendlyException("当前状态无法批准");

        Status = EcnStatus.Approved;
        ApprovedBy = approvedBy;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Execute()
    {
        if (Status != EcnStatus.Approved)
            throw new UserFriendlyException("只有已批准的ECN才能执行");

        Status = EcnStatus.Executed;
        ExecutedAt = DateTime.UtcNow;
    }

    public void SetDescription(string description)
    {
        Description = description;
    }

    public void SetPriority(Priority priority)
    {
        Priority = priority;
    }

    public void SetEcnNo(string ecnNo)
    {
        EcnNo = ecnNo ?? throw new ArgumentNullException(nameof(ecnNo));
    }

    public void SetTitle(string title)
    {
        Title = title ?? throw new ArgumentNullException(nameof(title));
    }

    public void SetType(ChangeType type)
    {
        Type = type;
    }

    public void SetAffectedProducts(List<Guid> ids)
    {
        AffectedProductIds = JsonSerializer.Serialize(ids);
    }

    public void SetAffectedBoms(List<Guid> ids)
    {
        AffectedBomVersionIds = JsonSerializer.Serialize(ids);
    }

    public void SetAffectedRoutes(List<Guid> ids)
    {
        AffectedProcessRouteIds = JsonSerializer.Serialize(ids);
    }
}