using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using AbpDemo.Domain.Shared.ValueObjects;
using AbpDemo.Enums;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace AbpDemo.Engineering.Products.Aggregates;

/// <summary>
/// 产品聚合根
/// </summary>
public class Product : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public string ProductCode { get; private set; }
    public string ProductName { get; private set; }
    public ProductType Type { get; private set; }
    public ProductSpecification Specification { get; private set; } // 值对象
    public string Material { get; private set; }
    public string Unit { get; private set; }
    public bool IsActive { get; private set; }

    // 光伏特有字段
    public bool IsSerialTraced { get; private set; } // 是否启用单件追溯
    public decimal? YieldRate { get; private set; } // 标准产出率（一对多转换）

    // 产品版本集合（子实体）
    private ICollection<ProductVersion> _versions = new List<ProductVersion>();
    public IReadOnlyCollection<ProductVersion> Versions => _versions.ToList();

    protected Product()
    {
    } // EF Core 需要

    public Product(Guid id, string code, string name, ProductType type) : base(id)
    {
        SetProductCode(code);
        SetProductName(name);
        Type = type;
        IsActive = true;
        IsSerialTraced = false; // 默认不启用序列号追溯
        YieldRate = null;
    }

    public void SetActive(bool isActive)
    {
        IsActive = isActive;
    }

    public void SetProductName(string productName)
    {
        ProductName = productName ?? throw new ArgumentNullException(nameof(productName));
    }

    public void SetType(ProductType type)
    {
        Type = type;
    }

    public void SetMaterial(string material)
    {
        Material = material;
    }

    public void SetUnit(string unit)
    {
        Unit = unit;
    }

    public void SetSpecification(ProductSpecification specification)
    {
        Specification = specification;
    }

    /// <summary>
    /// 设置产品编码（符合编码规范）
    /// </summary>
    public void SetProductCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentNullException(nameof(code));

        // 验证编码格式：^[A-Z]{2,4}-\d{4,8}$
        if (!Regex.IsMatch(code, @"^[A-Z]{2,4}-\d{4,8}$"))
            throw new ArgumentException("产品编码格式不正确，应为：XX-XXXX 或 XXX-XXXXXXXX");

        ProductCode = code;
    }

    /// <summary>
    /// 设置是否启用单件追溯（光伏特有）
    /// </summary>
    public void SetSerialTraced(bool isSerialTraced)
    {
        IsSerialTraced = isSerialTraced;
    }

    /// <summary>
    /// 设置标准产出率（光伏特有，用于一对多转换）
    /// </summary>
    public void SetYieldRate(decimal? yieldRate)
    {
        if (yieldRate.HasValue && yieldRate <= 0)
            throw new ArgumentException("产出率必须大于0");

        YieldRate = yieldRate;
    }

    // --- 领域方法 (Domain Methods) ---

    /// <summary>
    /// 创建新产品版本
    /// </summary>
    public ProductVersion CreateNewVersion(BomType type, string reason)
    {
        // 业务规则：同一产品的同一BomType只能有一个Active版本
        DeactivateOldVersions(type);

        // 自动生成版本号：基于该类型已有的版本数量
        var existingVersions = _versions.Count(v => v.BomType == type);
        var versionNo = $"V{existingVersions + 1}.0";

        var version = new ProductVersion(Guid.NewGuid(), Id, versionNo, type);
        version.SetChangeReason(reason);
        version.Activate();

        _versions.Add(version);
        return version;
    }

    /// <summary>
    /// 获取当前生效的版本
    /// </summary>
    public ProductVersion GetActiveVersion(BomType type)
    {
        return _versions.FirstOrDefault(v => v.BomType == type && v.IsActive);
    }

    /// <summary>
    /// 从上一版本克隆并创建新版本
    /// </summary>
    public ProductVersion CloneVersionFromPrevious(Guid previousVersionId, string newVersionNo, string reason)
    {
        var previousVersion = _versions.FirstOrDefault(v => v.Id == previousVersionId)
                              ?? throw new InvalidOperationException($"未找到版本 {previousVersionId}");

        // 停用旧版本
        DeactivateOldVersions(previousVersion.BomType);

        // 创建新版本
        var newVersion = new ProductVersion(Guid.NewGuid(), Id, newVersionNo, previousVersion.BomType);
        newVersion.SetChangeReason(reason);
        newVersion.Activate();

        // 复制 BOM 项
        foreach (var item in previousVersion.BomItems)
        {
            newVersion.AddBomItem(
                item.BomCode,
                item.ComponentProductId,
                item.ComponentProductName, // 冗余字段
                item.Quantity,
                item.ScrapRate,
                item.Unit,
                item.Sequence,
                item.ParentItemId,
                item.YieldRate
            );
        }

        _versions.Add(newVersion);
        return newVersion;
    }

    /// <summary>
    /// 停用同类型的旧版本
    /// </summary>
    private void DeactivateOldVersions(BomType type)
    {
        foreach (var version in _versions.Where(v => v.BomType == type && v.IsActive))
        {
            version.Deactivate();
        }
    }

    /// <summary>
    /// 删除指定版本
    /// </summary>
    public void RemoveVersion(Guid versionId)
    {
        var version = _versions.FirstOrDefault(v => v.Id == versionId);
        if (version != null)
        {
            // 不允许删除激活的版本
            if (version.IsActive)
                throw new InvalidOperationException("不能删除已激活的版本");

            _versions.Remove(version);
        }
    }

    /// <summary>
    /// 校验产品编码唯一性（需要在应用层调用仓储检查）
    /// </summary>
    public void ValidateProductCode()
    {
        if (string.IsNullOrWhiteSpace(ProductCode))
            throw new InvalidOperationException("产品编码不能为空");

        if (!Regex.IsMatch(ProductCode, @"^[A-Z]{2,4}-\d{4,8}$"))
            throw new InvalidOperationException("产品编码格式不正确");
    }
}