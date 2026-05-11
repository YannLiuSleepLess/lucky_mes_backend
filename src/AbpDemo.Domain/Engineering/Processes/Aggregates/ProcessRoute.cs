using System;
using System.Collections.Generic;
using System.Linq;
using AbpDemo.Enums;
using Volo.Abp.Domain.Entities.Auditing;

namespace AbpDemo.Engineering.Processes;

/// <summary>
/// 工艺路线聚合根
/// </summary>
public class ProcessRoute : FullAuditedAggregateRoot<Guid>
{
    public string RouteCode { get; private set; }
    public string RouteName { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid? BomVersionId { get; private set; }
    public string Version { get; private set; }
    public ProcessRouteStatus Status { get; private set; }
    public decimal TotalStandardTime { get; private set; }
    public Guid? PublishedBy { get; private set; } // 业务审计字段
    public DateTime? PublishedAt { get; private set; } // 业务审计字段

    // 子实体集合
    private ICollection<ProcessStep> _steps = new List<ProcessStep>();
    public IReadOnlyCollection<ProcessStep> Steps => _steps.ToList();

    private ICollection<ProcessParameter> _parameters = new List<ProcessParameter>();
    public IReadOnlyCollection<ProcessParameter> Parameters => _parameters.ToList();

    private ICollection<ProcessDocument> _documents = new List<ProcessDocument>();
    public IReadOnlyCollection<ProcessDocument> Documents => _documents.ToList();

    protected ProcessRoute()
    {
    }

    public ProcessRoute(Guid id, string code, string name, Guid productId, string version = "V1.0") : base(id)
    {
        SetRouteCode(code);
        SetRouteName(name);
        ProductId = productId;
        Version = version ?? throw new ArgumentNullException(nameof(version));
        Status = ProcessRouteStatus.Draft;
        TotalStandardTime = 0;
    }

    /// <summary>
    /// 发布工艺路线
    /// </summary>
    public void Publish(Guid publishedBy)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工艺路线才能发布");

        // 校验完整性
        if (!_steps.Any())
            throw new InvalidOperationException("工艺路线必须包含至少一个工序");

        // 校验关键工序的标准工时
        foreach (var step in _steps.Where(s => s.IsCritical))
        {
            if (step.StandardTime <= 0)
                throw new InvalidOperationException($"关键工序 {step.StepName} 的标准工时必须大于0");
        }

        Status = ProcessRouteStatus.Published;
        PublishedBy = publishedBy;
        PublishedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// 归档工艺路线
    /// </summary>
    public void Archive()
    {
        if (Status != ProcessRouteStatus.Published)
            throw new InvalidOperationException("只有已发布的工艺路线才能归档");

        Status = ProcessRouteStatus.Archived;
    }

    /// <summary>
    /// 添加工序（自动计算序号）
    /// </summary>
    public ProcessStep AddStep(string stepNo, string stepName, decimal standardTime, bool isCritical,
        Guid? equipmentTypeId = null, Guid? workCenterId = null, int? sequence = null,
        bool isInspectionRequired = false, InspectionType? inspectionType = null)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工艺路线才能添加工序");

        if (string.IsNullOrWhiteSpace(stepNo))
            throw new ArgumentNullException(nameof(stepNo));

        if (string.IsNullOrWhiteSpace(stepName))
            throw new ArgumentNullException(nameof(stepName));

        if (standardTime < 0)
            throw new ArgumentException("标准工时必须大于等于0");

        // 检查工序编号唯一性
        if (_steps.Any(s => s.StepNo == stepNo))
            throw new InvalidOperationException($"工序编号 {stepNo} 已存在");

        // 自动计算序号（步长为10）
        if (!sequence.HasValue)
        {
            sequence = _steps.Any() ? _steps.Max(s => s.Sequence) + 10 : 10;
        }

        var step = new ProcessStep(
            Guid.NewGuid(),
            Id,
            stepNo,
            stepName,
            standardTime,
            isCritical,
            equipmentTypeId,
            workCenterId,
            sequence.Value,
            isInspectionRequired,
            inspectionType
        );

        _steps.Add(step);
        RecalculateTotalStandardTime();
        return step;
    }

    /// <summary>
    /// 删除工序
    /// </summary>
    public void RemoveStep(Guid stepId)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工艺路线才能删除工序");

        var step = _steps.FirstOrDefault(s => s.Id == stepId);
        if (step != null)
        {
            _steps.Remove(step);
            RecalculateTotalStandardTime();
            RenumberSteps(); // 重新排序
        }
    }

    /// <summary>
    /// 清空所有工序
    /// </summary>
    public void ClearSteps()
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工艺路线才能清工序");

        _steps.Clear();
        TotalStandardTime = 0;
    }

    /// <summary>
    /// 克隆为新版本
    /// </summary>
    public ProcessRoute CloneAsNewVersion(string newVersionNo)
    {
        if (Status != ProcessRouteStatus.Published && Status != ProcessRouteStatus.Archived)
            throw new InvalidOperationException("只有已发布或已归档的工艺路线才能克隆");

        var newRoute = new ProcessRoute(
            Guid.NewGuid(),
            $"{RouteCode}-{newVersionNo.Replace(".", "")}",
            $"{RouteName} ({newVersionNo})",
            ProductId,
            newVersionNo
        );

        newRoute.BomVersionId = BomVersionId;

        // 复制工序
        foreach (var step in _steps.OrderBy(s => s.Sequence))
        {
            newRoute.AddStep(
                step.StepNo,
                step.StepName,
                step.StandardTime,
                step.IsCritical,
                step.EquipmentTypeId,
                step.WorkCenterId,
                step.Sequence,
                step.IsInspectionRequired,
                step.InspectionType
            );
        }

        return newRoute;
    }

    /// <summary>
    /// 添加工艺参数
    /// </summary>
    public void AddParameter(ProcessParameter parameter)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态的工艺路线才能添加参数");

        _parameters.Add(parameter);
    }

    /// <summary>
    /// 添加工艺文档
    /// </summary>
    public void AddDocument(ProcessDocument document)
    {
        _documents.Add(document);
    }

    public void SetRouteCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        // 验证编码格式：PR-{ProductCode}-{SEQ}
        if (!code.StartsWith("PR-"))
            throw new ArgumentException("工艺路线编码必须以PR-开头");

        RouteCode = code;
    }

    public void SetRouteName(string name)
    {
        RouteName = name ?? throw new ArgumentNullException(nameof(name));
    }

    public void SetProductId(Guid productId)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态才能修改关联产品");

        ProductId = productId;
    }

    public void SetBomVersionId(Guid? bomVersionId)
    {
        if (Status != ProcessRouteStatus.Draft)
            throw new InvalidOperationException("只有草稿状态才能修改关联BOM版本");

        BomVersionId = bomVersionId;
    }

    #region Private Methods

    /// <summary>
    /// 重新计算总标准工时
    /// </summary>
    private void RecalculateTotalStandardTime()
    {
        TotalStandardTime = _steps.Sum(s => s.StandardTime);
    }

    /// <summary>
    /// 重新排序工序序号
    /// </summary>
    private void RenumberSteps()
    {
        var orderedSteps = _steps.OrderBy(s => s.Sequence).ToList();
        for (int i = 0; i < orderedSteps.Count; i++)
        {
            orderedSteps[i].SetSequence((i + 1) * 10);
        }
    }

    #endregion
}