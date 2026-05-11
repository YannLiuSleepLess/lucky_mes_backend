using System;

namespace AbpDemo.Engineering.Products.Dtos;

public class BomItemDto
{
    public Guid Id { get; set; }
    public Guid? ParentItemId { get; set; }
    public Guid ComponentProductId { get; set; }
    public string ComponentProductName { get; set; } // 冗余字段，方便前端展示
    public decimal Quantity { get; set; }
    public decimal ScrapRate { get; set; }
    public string Unit { get; set; }
    public int Sequence { get; set; }
    public int Level { get; set; }
}