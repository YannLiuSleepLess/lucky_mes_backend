using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpDemo.BasicData.WorkCenters.Aggregates;

/// <summary>
/// 工作中心聚合根
/// </summary>
public class WorkCenter : FullAuditedAggregateRoot<Guid>
{
    /// <summary>
    /// 工作中心编码（格式：WC-{WorkshopCode}-{SEQ}）
    /// </summary>
    public string WorkCenterCode { get; private set; }

    /// <summary>
    /// 工作中心名称
    /// </summary>
    public string WorkCenterName { get; private set; }

    /// <summary>
    /// 所属车间ID
    /// </summary>
    public Guid WorkshopId { get; private set; }

    /// <summary>
    /// 产能（件/班）
    /// </summary>
    public int Capacity { get; private set; }

    /// <summary>
    /// 班次数量
    /// </summary>
    public int ShiftCount { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; private set; }

    protected WorkCenter()
    {
    }

    public WorkCenter(Guid id, string workCenterCode, string workCenterName, Guid workshopId) : base(id)
    {
        SetWorkCenterCode(workCenterCode);
        SetWorkCenterName(workCenterName);
        SetWorkshopId(workshopId);
        IsActive = true;
        ShiftCount = 1;
    }

    public void SetWorkCenterCode(string workCenterCode)
    {
        if (string.IsNullOrWhiteSpace(workCenterCode))
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkCenterCodeRequired);

        WorkCenterCode = workCenterCode.Trim();
    }

    public void SetWorkCenterName(string workCenterName)
    {
        if (string.IsNullOrWhiteSpace(workCenterName))
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkCenterNameRequired);

        WorkCenterName = workCenterName.Trim();
    }

    public void SetWorkshopId(Guid workshopId)
    {
        WorkshopId = workshopId;
    }

    public void SetCapacity(int capacity)
    {
        if (capacity <= 0)
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkCenterCapacityInvalid);

        Capacity = capacity;
    }

    public void SetShiftCount(int shiftCount)
    {
        if (shiftCount < 1 || shiftCount > 3)
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkCenterShiftCountInvalid);

        ShiftCount = shiftCount;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}