using System;
using System.Collections.Generic;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.Products.Dtos;

public class ProductVersionDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string VersionNo { get; set; }
    public BomType BomType { get; set; }
    public string ChangeReason { get; set; }
    public bool IsActive { get; set; }
    public DateTime? ChangedAt { get; set; }

    // BOM 项列表
    public List<BomItemDto> BomItems { get; set; } = new List<BomItemDto>();
}