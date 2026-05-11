using System;
using System.Collections.Generic;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.Changes.Dtos;

public class EngineeringChangeDto
{
    public Guid Id { get; set; }
    public string EcnNo { get; set; }
    public string Title { get; set; }
    public ChangeType Type { get; set; }
    public string Description { get; set; }
    public EcnStatus Status { get; set; }
    public Priority Priority { get; set; }

    // 关联的 ID 列表（用于展示）
    public List<Guid> AffectedProductIds { get; set; } = new();
    public List<Guid> AffectedBomVersionIds { get; set; } = new();
    public List<Guid> AffectedProcessRouteIds { get; set; } = new();

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public Guid? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime? ExecutedAt { get; set; }
}