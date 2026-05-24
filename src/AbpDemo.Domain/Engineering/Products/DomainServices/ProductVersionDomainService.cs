using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.Engineering.DomainServices;
using AbpDemo.Engineering.Products.Repositories;
using AbpDemo.Enums;
using Volo.Abp.Domain.Services;

namespace AbpDemo.Engineering.Products.DomainServices;

/// <summary>
/// 产品版本领域服务实现
/// </summary>
public class ProductVersionDomainService : DomainService, IProductVersionDomainService
{
    private readonly IProductRepository _productRepository;

    public ProductVersionDomainService(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    /// <summary>
    /// 比较两个版本的差异
    /// </summary>
    public async Task<BomVersionComparison> CompareVersionsAsync(Guid version1Id, Guid version2Id)
    {
        var version1 = await GetVersionWithDetailsAsync(version1Id);
        var version2 = await GetVersionWithDetailsAsync(version2Id);

        var comparison = new BomVersionComparison
        {
            Version1Id = version1Id,
            Version2Id = version2Id,
            Version1No = version1.VersionNo,
            Version2No = version2.VersionNo
        };

        var items1 = version1.BomItems.ToDictionary(i => i.ComponentProductId, i => i);
        var items2 = version2.BomItems.ToDictionary(i => i.ComponentProductId, i => i);

        // 查找新增的项（在version2中存在，version1中不存在）
        foreach (var item2 in items2)
        {
            if (!items1.ContainsKey(item2.Key))
            {
                comparison.AddedItems.Add(new BomItemDifference
                {
                    ComponentProductId = item2.Key,
                    Type = DifferenceType.Added,
                    NewValue = CreateSnapshot(item2.Value)
                });
            }
        }

        // 查找删除的项（在version1中存在，version2中不存在）
        foreach (var item1 in items1)
        {
            if (!items2.ContainsKey(item1.Key))
            {
                comparison.RemovedItems.Add(new BomItemDifference
                {
                    ComponentProductId = item1.Key,
                    Type = DifferenceType.Removed,
                    OldValue = CreateSnapshot(item1.Value)
                });
            }
        }

        // 查找修改的项（两个版本中都存在，但属性有变化）
        foreach (var item1 in items1)
        {
            if (items2.TryGetValue(item1.Key, out var item2))
            {
                if (IsItemModified(item1.Value, item2))
                {
                    comparison.ModifiedItems.Add(new BomItemDifference
                    {
                        ComponentProductId = item1.Key,
                        Type = DifferenceType.Modified,
                        OldValue = CreateSnapshot(item1.Value),
                        NewValue = CreateSnapshot(item2)
                    });
                }
            }
        }

        return comparison;
    }

    /// <summary>
    /// 获取产品的生效版本
    /// </summary>
    public async Task<ProductVersion> GetEffectiveVersionAsync(Guid productId, BomType type)
    {
        var product = await _productRepository.GetWithVersionsAsync(productId);
        var version = product.GetActiveVersion(type);

        if (version == null)
        {
            throw new InvalidOperationException($"产品 {productId} 没有生效的 {type} 版本");
        }

        return version;
    }

    /// <summary>
    /// 校验BOM完整性（包括循环引用检查）
    /// </summary>
    public async Task<bool> ValidateBomIntegrityAsync(ProductVersion version, List<string> errors)
    {
        // 首先进行基本校验
        var basicValidationResult = version.ValidateBomIntegrity(out var basicErrors);
        errors.AddRange(basicErrors);

        // 检查循环引用
        if (await HasCircularReference(version))
        {
            errors.Add("BOM 结构中存在循环引用");
        }

        return errors.Count == 0;
    }

    #region Private Methods

    private async Task<ProductVersion> GetVersionWithDetailsAsync(Guid versionId)
    {
        var product = await _productRepository.GetVersionWithBomItemsAsync(versionId);
        var version = product.Versions.FirstOrDefault(v => v.Id == versionId);

        if (version == null)
        {
            throw new InvalidOperationException($"未找到版本 {versionId}");
        }

        return version;
    }

    private BomItemSnapshot CreateSnapshot(BomItem item)
    {
        return new BomItemSnapshot
        {
            Quantity = item.Quantity,
            ScrapRate = item.ScrapRate,
            Unit = item.Unit,
            Sequence = item.Sequence,
            YieldRate = item.YieldRate
        };
    }

    private bool IsItemModified(BomItem item1, BomItem item2)
    {
        return item1.Quantity != item2.Quantity ||
               item1.ScrapRate != item2.ScrapRate ||
               item1.Unit != item2.Unit ||
               item1.Sequence != item2.Sequence ||
               item1.YieldRate != item2.YieldRate;
    }

    /// <summary>
    /// 检查是否存在循环引用
    /// </summary>
    private async Task<bool> HasCircularReference(ProductVersion version)
    {
        var bomItems = version.BomItems.ToList();
        var visited = new HashSet<Guid>();
        var recursionStack = new HashSet<Guid>();

        foreach (var item in bomItems.Where(i => !i.ParentItemId.HasValue))
        {
            if (await HasCycleHelper(item.ComponentProductId, bomItems, visited, recursionStack))
            {
                return true;
            }
        }

        return false;
    }

    private async Task<bool> HasCycleHelper(
        Guid productId,
        List<BomItem> bomItems,
        HashSet<Guid> visited,
        HashSet<Guid> recursionStack)
    {
        if (recursionStack.Contains(productId))
        {
            return true; // 发现循环
        }

        if (visited.Contains(productId))
        {
            return false; // 已经访问过，无循环
        }

        visited.Add(productId);
        recursionStack.Add(productId);

        // 查找该产品的所有子项
        var children = bomItems.Where(i => i.ComponentProductId == productId).ToList();

        foreach (var child in children)
        {
            if (await HasCycleHelper(child.ComponentProductId, bomItems, visited, recursionStack))
            {
                return true;
            }
        }

        recursionStack.Remove(productId);
        return false;
    }

    #endregion
}