using System;
using System.Threading.Tasks;
using AbpDemo.Domain.Shared.ValueObjects;
using AbpDemo.Engineering.Products;
using AbpDemo.Enums;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace AbpDemo.Domain.Seeds;

/// <summary>
/// 光伏MES系统产品数据种子贡献者
/// 初始化光伏产业链的典型产品及其BOM配置
/// </summary>
public class PhotovoltaicProductDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PhotovoltaicProductDataSeedContributor(
        IRepository<Product, Guid> productRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _productRepository = productRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // 检查是否已经存在光伏产品数据
        if (await _productRepository.CountAsync() > 0)
        {
            return; // 已有数据，跳过种子
        }

        // ========================================
        // 第一步：创建所有产品（原材料、半成品、成品）
        // ========================================

        // --- 原材料层 ---
        var polySilicon =
            await CreateProductAsync("RM-10001", "多晶硅料", ProductType.RawMaterial, "KG", isSerialTraced: false);
        polySilicon.SetSpecification(
            new ProductSpecification(null, null, null, null, null, "太阳能级多晶硅"));

        var silverPaste =
            await CreateProductAsync("RM-10002", "银浆", ProductType.RawMaterial, "G", isSerialTraced: false);
        silverPaste.SetSpecification(
            new ProductSpecification(null, null, null, null, null, "正面导电银浆"));

        var aluminumPaste =
            await CreateProductAsync("RM-10003", "铝浆", ProductType.RawMaterial, "G", isSerialTraced: false);
        aluminumPaste.SetSpecification(
            new ProductSpecification(null, null, null, null, null, "背面导电铝浆"));

        var evaFilm =
            await CreateProductAsync("RM-10004", "EVA胶膜", ProductType.RawMaterial, "M2", isSerialTraced: false);
        evaFilm.SetSpecification(
            new ProductSpecification(null, null, null, 0.5m, null, "乙烯-醋酸乙烯共聚物"));

        var temperedGlass =
            await CreateProductAsync("RM-10005", "钢化玻璃", ProductType.RawMaterial, "PCS", isSerialTraced: false);
        temperedGlass.SetSpecification(
            new ProductSpecification(1650m, 992m, 3.2m, null, null, "超白布纹钢化玻璃"));

        var backSheet =
            await CreateProductAsync("RM-10006", "背板", ProductType.RawMaterial, "PCS", isSerialTraced: false);
        backSheet.SetSpecification(
            new ProductSpecification(1650m, 992m, 0.35m, null, null, "TPT复合背板"));

        var alFrame = await CreateProductAsync("RM-10007", "铝边框", ProductType.RawMaterial, "M", isSerialTraced: false);
        alFrame.SetSpecification(
            new ProductSpecification(null, null, null, 1.2m, null, "阳极氧化铝合金"));

        var junctionBox =
            await CreateProductAsync("RM-10008", "接线盒", ProductType.RawMaterial, "PCS", isSerialTraced: false);
        junctionBox.SetSpecification(
            new ProductSpecification(null, null, null, null, null, "IP67防护等级"));

        // --- 半成品层 ---
        var ingot = await CreateProductAsync("SF-20001", "单晶硅棒", ProductType.Component, "PCS", isSerialTraced: true,
            yieldRate: null);
        ingot.SetSpecification(
            new ProductSpecification(2000m, 210m, 210m, null, 150m, "P型单晶"));

        var wafer = await CreateProductAsync("SF-20002", "硅片", ProductType.Component, "PCS", isSerialTraced: true,
            yieldRate: 1000m); // 1根硅棒→1000片硅片
        wafer.SetSpecification(
            new ProductSpecification(165m, 165m, 0.18m, null, 5m, "166mm单晶硅片"));

        var cell = await CreateProductAsync("SF-20003", "PERC电池片", ProductType.Component, "PCS", isSerialTraced: true,
            yieldRate: 950m); // 1000硅片→950电池片
        cell.SetSpecification(
            new ProductSpecification(165m, 165m, 0.18m, null, 6m, "PERC高效电池"));

        // --- 成品层 ---
        var module500W = await CreateProductAsync("FG-30001", "500W单晶PERC组件", ProductType.FinishedGood, "PCS",
            isSerialTraced: true, yieldRate: null);
        module500W.SetSpecification(
            new ProductSpecification(2094m, 1038m, 40m, null, 27m, "72片单晶PERC组件"));

        var module550W = await CreateProductAsync("FG-30002", "550W单晶PERC组件", ProductType.FinishedGood, "PCS",
            isSerialTraced: true, yieldRate: null);
        module550W.SetSpecification(
            new ProductSpecification(2279m, 1134m, 40m, null, 30m, "78片单晶PERC组件"));

        // ========================================
        // 第二步：为各产品创建版本和BOM
        // ========================================

        // --- 硅棒的BOM（由多晶硅料制成）---
        var ingotVersion = ingot.CreateNewVersion(BomType.MBOM, "初始版本");
        ingotVersion.AddBomItem(polySilicon.Id, polySilicon.ProductName, quantity: 2.5m, scrapRate: 0.02m, unit: "KG",
            sequence: 10);

        // --- 硅片的BOM（由硅棒切割而成）---
        var waferVersion = wafer.CreateNewVersion(BomType.MBOM, "初始版本");
        waferVersion.AddBomItem(ingot.Id, ingot.ProductName, quantity: 0.001m, scrapRate: 0.03m, unit: "PCS",
            sequence: 10, yieldRate: 1000m); // 1根硅棒产出1000片

        // --- 电池片的BOM（由硅片加工而成）---
        var cellVersion = cell.CreateNewVersion(BomType.MBOM, "初始版本");
        cellVersion.AddBomItem(wafer.Id, wafer.ProductName, quantity: 1m, scrapRate: 0.05m, unit: "PCS", sequence: 10,
            yieldRate: 1m);
        cellVersion.AddBomItem(silverPaste.Id, silverPaste.ProductName, quantity: 0.05m, scrapRate: 0.02m, unit: "G",
            sequence: 20); // 正面银浆
        cellVersion.AddBomItem(aluminumPaste.Id, aluminumPaste.ProductName, quantity: 0.08m, scrapRate: 0.02m,
            unit: "G", sequence: 30); // 背面铝浆

        // --- 500W组件的BOM ---
        var module500WVersion = module500W.CreateNewVersion(BomType.MBOM, "初始版本");
        module500WVersion.AddBomItem(cell.Id, cell.ProductName, quantity: 72m, scrapRate: 0.02m, unit: "PCS",
            sequence: 10); // 72片电池
        module500WVersion.AddBomItem(temperedGlass.Id, temperedGlass.ProductName, quantity: 1m, scrapRate: 0.01m,
            unit: "PCS", sequence: 20);
        module500WVersion.AddBomItem(evaFilm.Id, evaFilm.ProductName, quantity: 2m, scrapRate: 0.01m, unit: "PCS",
            sequence: 30); // 上下两层
        module500WVersion.AddBomItem(backSheet.Id, backSheet.ProductName, quantity: 1m, scrapRate: 0.01m, unit: "PCS",
            sequence: 40);
        module500WVersion.AddBomItem(alFrame.Id, alFrame.ProductName, quantity: 4m, scrapRate: 0.01m, unit: "PCS",
            sequence: 50); // 4根边框
        module500WVersion.AddBomItem(junctionBox.Id, junctionBox.ProductName, quantity: 1m, scrapRate: 0.005m,
            unit: "PCS", sequence: 60);

        // --- 550W组件的BOM ---
        var module550WVersion = module550W.CreateNewVersion(BomType.MBOM, "初始版本");
        module550WVersion.AddBomItem(cell.Id, cell.ProductName, quantity: 78m, scrapRate: 0.02m, unit: "PCS",
            sequence: 10); // 78片电池
        module550WVersion.AddBomItem(temperedGlass.Id, temperedGlass.ProductName, quantity: 1m, scrapRate: 0.01m,
            unit: "PCS", sequence: 20);
        module550WVersion.AddBomItem(evaFilm.Id, evaFilm.ProductName, quantity: 2m, scrapRate: 0.01m, unit: "PCS",
            sequence: 30);
        module550WVersion.AddBomItem(backSheet.Id, backSheet.ProductName, quantity: 1m, scrapRate: 0.01m, unit: "PCS",
            sequence: 40);
        module550WVersion.AddBomItem(alFrame.Id, alFrame.ProductName, quantity: 4m, scrapRate: 0.01m, unit: "PCS",
            sequence: 50);
        module550WVersion.AddBomItem(junctionBox.Id, junctionBox.ProductName, quantity: 1m, scrapRate: 0.005m,
            unit: "PCS", sequence: 60);

        // 保存所有更改
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    /// <summary>
    /// 创建产品并保存到数据库
    /// </summary>
    private async Task<Product> CreateProductAsync(
        string code,
        string name,
        ProductType type,
        string unit,
        bool isSerialTraced = false,
        decimal? yieldRate = null,
        string? material = null)
    {
        var product = new Product(Guid.NewGuid(), code, name, type);
        product.SetUnit(unit);
        product.SetSerialTraced(isSerialTraced);

        // 如果没有指定材质，使用产品名称作为默认值
        product.SetMaterial(material ?? name);

        if (yieldRate.HasValue)
        {
            product.SetYieldRate(yieldRate);
        }

        await _productRepository.InsertAsync(product);
        return product;
    }
}