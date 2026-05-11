using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.Changes.Dtos;

public class CreateEngineeringChangeRequest
{
    [Required] public string EcnNo { get; set; }

    [Required] public string Title { get; set; }

    [Required] public ChangeType Type { get; set; }

    public string Description { get; set; }

    public Priority Priority { get; set; } = Priority.Medium;

    // 关联的 ID 列表
    public List<Guid> AffectedProductIds { get; set; } = new();
    public List<Guid> AffectedBomVersionIds { get; set; } = new();
    public List<Guid> AffectedProcessRouteIds { get; set; } = new();
}