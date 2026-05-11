using System.ComponentModel.DataAnnotations;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.Products.Dtos;

public class UpdateProductRequest
{
    [Required] [StringLength(200)] public string ProductName { get; set; }

    public ProductType Type { get; set; }

    public ProductSpecificationDto Specification { get; set; }

    [StringLength(100)] public string Material { get; set; }

    [StringLength(20)] public string Unit { get; set; }

    public bool IsActive { get; set; }
}