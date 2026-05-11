using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AbpDemo.Engineering.Products;
using AbpDemo.Enums;

namespace AbpDemo.Engineering.DomainServices;

/// <summary>
/// 产品版本领域服务接口
/// </summary>
public interface IProductVersionDomainService
{
    /// <summary>
    /// 比较两个版本的差异
    /// </summary>
    Task<BomVersionComparison> CompareVersionsAsync(Guid version1Id, Guid version2Id);

    /// <summary>
    /// 获取产品的生效版本
    /// </summary>
    Task<ProductVersion> GetEffectiveVersionAsync(Guid productId, BomType type);

    /// <summary>
    /// 校验BOM完整性（包括循环引用检查）
    /// </summary>
    Task<bool> ValidateBomIntegrityAsync(ProductVersion version, List<string> errors);
}