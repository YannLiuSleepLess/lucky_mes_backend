using System;
using System.Collections.Generic;
using AbpDemo.Enums;
using Volo.Abp.Application.Dtos;

namespace AbpDemo.Engineering.Products.Dtos;

public class ProductDto : AuditedEntityDto<Guid>
{
    public string ProductCode { get; set; }
    public string ProductName { get; set; }
    public ProductType Type { get; set; }
    public ProductSpecificationDto Specification { get; set; }
    public string Material { get; set; }
    public string Unit { get; set; }
    public bool IsActive { get; set; }

    // BOM 相关
    public List<BomItemDto> BomItems { get; set; } = new();
    public List<ProductVersionDto> Versions { get; set; } = new();
}