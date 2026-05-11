using System;

namespace AbpDemo.Engineering.Products.Dtos;

public class UpdateBomItemRequest
{
    public Guid ComponentProductId { get; set; }
    public decimal Quantity { get; set; }
    public decimal ScrapRate { get; set; }
    public string Unit { get; set; }
    public int Sequence { get; set; }
    public Guid? ParentItemId { get; set; }
}