using System;
using System.Collections.Generic;

namespace AbpDemo.Engineering.Products;

/// <summary>
/// BOM版本对比结果
/// </summary>
public class BomVersionComparison
{
    public Guid Version1Id { get; set; }
    public Guid Version2Id { get; set; }
    public string Version1No { get; set; }
    public string Version2No { get; set; }

    /// <summary>
    /// 新增的BOM项（在Version2中存在，Version1中不存在）
    /// </summary>
    public List<BomItemDifference> AddedItems { get; set; } = new List<BomItemDifference>();

    /// <summary>
    /// 删除的BOM项（在Version1中存在，Version2中不存在）
    /// </summary>
    public List<BomItemDifference> RemovedItems { get; set; } = new List<BomItemDifference>();

    /// <summary>
    /// 修改的BOM项（两个版本中都存在，但属性有变化）
    /// </summary>
    public List<BomItemDifference> ModifiedItems { get; set; } = new List<BomItemDifference>();
}

/// <summary>
/// BOM项差异
/// </summary>
public class BomItemDifference
{
    public Guid ComponentProductId { get; set; }
    public string ComponentProductCode { get; set; }
    public string ComponentProductName { get; set; }

    /// <summary>
    /// 差异类型
    /// </summary>
    public DifferenceType Type { get; set; }

    /// <summary>
    /// 旧值（Version1）
    /// </summary>
    public BomItemSnapshot OldValue { get; set; }

    /// <summary>
    /// 新值（Version2）
    /// </summary>
    public BomItemSnapshot NewValue { get; set; }
}

/// <summary>
/// BOM项快照
/// </summary>
public class BomItemSnapshot
{
    public decimal Quantity { get; set; }
    public decimal ScrapRate { get; set; }
    public string Unit { get; set; }
    public int Sequence { get; set; }
    public decimal? YieldRate { get; set; }
}

/// <summary>
/// 差异类型
/// </summary>
public enum DifferenceType
{
    Added, // 新增
    Removed, // 删除
    Modified // 修改
}