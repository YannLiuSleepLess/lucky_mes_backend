using AbpDemo.Enums;

namespace AbpDemo.Engineering.Products.Dtos;

public class UpdateProductVersionRequest
{
    public string VersionNo { get; set; }
    public BomType BomType { get; set; }
    public string ChangeReason { get; set; }
    public bool IsActive { get; set; }
}