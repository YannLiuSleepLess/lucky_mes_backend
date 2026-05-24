using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AbpDemo.BasicData.Workshops.Aggregates;

/// <summary>
/// 车间聚合根
/// </summary>
public class Workshop : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }

    /// <summary>
    /// 车间编码（格式：WS-{SEQ}）
    /// </summary>
    public string WorkshopCode { get; private set; }

    /// <summary>
    /// 车间名称
    /// </summary>
    public string WorkshopName { get; private set; }

    /// <summary>
    /// 位置
    /// </summary>
    public string Location { get; private set; }

    /// <summary>
    /// 车间主任ID
    /// </summary>
    public Guid? ManagerId { get; private set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsActive { get; private set; }

    protected Workshop()
    {
    }

    public Workshop(Guid id, string workshopCode, string workshopName) : base(id)
    {
        SetWorkshopCode(workshopCode);
        SetWorkshopName(workshopName);
        IsActive = true;
    }

    public void SetWorkshopCode(string workshopCode)
    {
        if (string.IsNullOrWhiteSpace(workshopCode))
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkshopCodeRequired);

        WorkshopCode = workshopCode.Trim();
    }

    public void SetWorkshopName(string workshopName)
    {
        if (string.IsNullOrWhiteSpace(workshopName))
            throw new BusinessException(AbpDemoDomainErrorCodes.WorkshopNameRequired);

        WorkshopName = workshopName.Trim();
    }

    public void SetLocation(string location)
    {
        Location = location?.Trim();
    }

    public void SetManager(Guid? managerId)
    {
        ManagerId = managerId;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }
}