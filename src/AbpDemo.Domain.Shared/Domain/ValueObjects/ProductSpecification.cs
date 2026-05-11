namespace AbpDemo.Domain.Shared.ValueObjects;

public class ProductSpecification
{
    public decimal? Length { get; private set; }
    public decimal? Width { get; private set; }
    public decimal? Height { get; private set; }
    public decimal? Thickness { get; private set; }
    public decimal? Weight { get; private set; }
    public string CustomSpecs { get; private set; }

    protected ProductSpecification()
    {
    }

    public ProductSpecification(decimal? length, decimal? width, decimal? height, decimal? thickness, decimal? weight,
        string customSpecs)
    {
        Length = length;
        Width = width;
        Height = height;
        Thickness = thickness;
        Weight = weight;
        CustomSpecs = customSpecs;
    }
}