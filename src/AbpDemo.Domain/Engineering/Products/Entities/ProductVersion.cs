using System;
using System.Collections.Generic;
using System.Linq;
using AbpDemo.Enums;
using Volo.Abp.Domain.Entities;

namespace AbpDemo.Engineering.Products;

/// <summary>
/// 产品版本 (子实体) - 属于 Product 聚合根
/// 每个产品版本对应一个完整的 BOM 结构
/// </summary>
public class ProductVersion : Entity<Guid>
{
    public Guid ProductId { get; private set; }
    public string VersionNo { get; private set; } // 如 V1.0, V2.0
    public BomType BomType { get; private set; } // EBOM, MBOM, PBOM
    public string ChangeReason { get; private set; }
    public Guid? ChangedBy { get; private set; } // 业务审计字段
    public DateTime? ChangedAt { get; private set; }
    public bool IsActive { get; private set; } // 同一类型只能有一个生效

    // 该版本对应的 BOM 项列表
    private ICollection<BomItem> _bomItems = new List<BomItem>();
    public IReadOnlyCollection<BomItem> BomItems => _bomItems.ToList();

    protected ProductVersion()
    {
    }

    public ProductVersion(Guid id, Guid productId, string versionNo, BomType type) : base(id)
    {
        ProductId = productId;
        VersionNo = versionNo ?? throw new ArgumentNullException(nameof(versionNo));
        BomType = type;
        IsActive = false;
    }

    public void Activate()
    {
        IsActive = true;
        ChangedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
    }

    public void SetChangeReason(string reason)
    {
        ChangeReason = reason;
    }

    public void SetVersionNo(string versionNo)
    {
        VersionNo = versionNo ?? throw new ArgumentNullException(nameof(versionNo));
    }

    public void SetBomType(BomType type)
    {
        BomType = type;
    }

    /// <summary>
    /// 添加 BOM 项到该版本
    /// </summary>
    public BomItem AddBomItem(Guid componentProductId, string componentProductName, decimal quantity,
        decimal scrapRate = 0,
        string unit = null, int sequence = 0, Guid? parentItemId = null, decimal? yieldRate = null)
    {
        if (quantity <= 0) throw new ArgumentException("用量必须大于0");
        if (scrapRate < 0 || scrapRate > 1) throw new ArgumentException("损耗率必须在0-1之间");
        if (string.IsNullOrWhiteSpace(componentProductName)) throw new ArgumentException("组件产品名称不能为空");

        // 自动计算序号
        if (sequence == 0)
        {
            sequence = _bomItems.Any() ? _bomItems.Max(i => i.Sequence) + 10 : 10;
        }

        var item = new BomItem(
            Guid.NewGuid(),
            Id, // ProductVersionId
            componentProductId,
            componentProductName, // 冗余字段
            quantity,
            scrapRate,
            unit ?? "PCS",
            sequence,
            parentItemId,
            yieldRate
        );

        _bomItems.Add(item);
        return item;
    }

    /// <summary>
    /// 从该版本删除 BOM 项
    /// </summary>
    public void RemoveBomItem(Guid itemId)
    {
        var item = _bomItems.FirstOrDefault(i => i.Id == itemId);
        if (item != null)
        {
            _bomItems.Remove(item);
        }
    }

    /// <summary>
    /// 获取该版本的完整 BOM 树结构（简化版，实际应由领域服务实现）
    /// </summary>
    public IEnumerable<BomItem> GetBomTree()
    {
        return _bomItems.OrderBy(i => i.Sequence);
    }

    /// <summary>
    /// 校验该版本 BOM 完整性
    /// </summary>
    public bool ValidateBomIntegrity(out List<string> errors)
    {
        errors = new List<string>();

        // 检查是否有 BOM 项
        if (!_bomItems.Any())
        {
            errors.Add("BOM 不能为空");
        }

        // 检查用量
        foreach (var item in _bomItems)
        {
            if (item.Quantity <= 0)
            {
                errors.Add($"BOM 项 {item.ComponentProductId} 的用量必须大于0");
            }

            if (item.ScrapRate < 0 || item.ScrapRate > 1)
            {
                errors.Add($"BOM 项 {item.ComponentProductId} 的损耗率必须在0-1之间");
            }
        }

        // 检查序号唯一性
        var sequences = _bomItems.Select(i => i.Sequence).ToList();
        if (sequences.Count != sequences.Distinct().Count())
        {
            errors.Add("BOM 项序号必须唯一");
        }

        // TODO: 检查循环引用（需要更复杂的算法）

        return errors.Count == 0;
    }
}