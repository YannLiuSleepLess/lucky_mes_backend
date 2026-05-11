namespace AbpDemo.Engineering.Products.Dtos;

public class ProductSpecificationDto
{
    public decimal? Length { get; set; }
    public decimal? Width { get; set; }
    public decimal? Height { get; set; }
    public decimal? Thickness { get; set; }
    public decimal? Weight { get; set; }
    public string CustomSpecs { get; set; }
}