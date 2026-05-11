using System;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Products;

/// <summary>
/// BOM 项 (子实体) - 属于 ProductVersion（产品版本）
/// </summary>
public class BomItem : Entity<Guid>
{
    public Guid ProductVersionId { get; private set; } // 指向产品版本，不是产品
    public Guid? ParentItemId { get; private set; } // 自引用，支持树形结构
    public Guid ComponentProductId { get; private set; } // 关联的组件产品ID
    public string ComponentProductName { get; private set; } // 冗余字段：组件产品名称（创建时从Product复制）
    public decimal Quantity { get; private set; }
    public decimal ScrapRate { get; private set; }
    public string Unit { get; private set; }
    public int Sequence { get; private set; }
    public int Level { get; private set; } // 计算字段

    // 光伏特有字段
    public decimal? YieldRate { get; private set; } // 产出率（一对多转换：1根硅棒→1000片硅片，则=1000）

    protected BomItem()
    {
    }

    public BomItem(Guid id, Guid productVersionId, Guid componentProductId, string componentProductName, decimal quantity, decimal scrapRate,
        string unit, int sequence, Guid? parentItemId = null, decimal? yieldRate = null) : base(id)
    {
        ProductVersionId = productVersionId;
        ComponentProductId = componentProductId;
        ComponentProductName = componentProductName ?? throw new ArgumentNullException(nameof(componentProductName));
        SetQuantity(quantity);
        SetScrapRate(scrapRate);
        Unit = unit;
        Sequence = sequence;
        ParentItemId = parentItemId;
        Level = parentItemId.HasValue ? 2 : 1; // 简化处理，实际应由领域服务计算
        SetYieldRate(yieldRate);
    }

    public void SetQuantity(decimal quantity)
    {
        if (quantity <= 0) throw new ArgumentException("用量必须大于0");
        Quantity = quantity;
    }

    public void SetScrapRate(decimal scrapRate)
    {
        if (scrapRate < 0 || scrapRate > 1) throw new ArgumentException("损耗率必须在0-1之间");
        ScrapRate = scrapRate;
    }

    /// <summary>
    /// 设置产出率（光伏特有）
    /// </summary>
    public void SetYieldRate(decimal? yieldRate)
    {
        if (yieldRate.HasValue && yieldRate <= 0)
            throw new ArgumentException("产出率必须大于0");

        YieldRate = yieldRate;
    }
}