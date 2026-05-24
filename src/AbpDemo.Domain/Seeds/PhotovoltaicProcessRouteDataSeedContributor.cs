using System;
using System.Linq;
using System.Threading.Tasks;
using AbpDemo.Engineering.Processes;
using AbpDemo.Engineering.Products.Aggregates;
using AbpDemo.Enums;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace AbpDemo.Seeds;

/// <summary>
/// 光伏MES系统工艺路线数据种子贡献者
/// 根据已有的产品和BOM数据，初始化典型的光伏工艺路线
/// </summary>
public class PhotovoltaicProcessRouteDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Product, Guid> _productRepository;
    private readonly IRepository<ProcessRoute, Guid> _processRouteRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public PhotovoltaicProcessRouteDataSeedContributor(
        IRepository<Product, Guid> productRepository,
        IRepository<ProcessRoute, Guid> processRouteRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _productRepository = productRepository;
        _processRouteRepository = processRouteRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        // 检查是否已经存在工艺路线数据
        if (await _processRouteRepository.CountAsync() > 0)
        {
            return; // 已有数据，跳过种子
        }

        // 获取所有产品
        var products = await _productRepository.GetListAsync();
        var productDict = products.ToDictionary(p => p.ProductCode);

        // ========================================
        // 创建各产品的工艺路线
        // ========================================

        // --- 1. 单晶硅棒工艺路线（由多晶硅料制成）---
        if (productDict.TryGetValue("SF-20001", out var ingot))
        {
            await CreateIngotProcessRouteAsync(ingot.Id);
        }

        // --- 2. 硅片工艺路线（由硅棒切割而成）---
        if (productDict.TryGetValue("SF-20002", out var wafer))
        {
            await CreateWaferProcessRouteAsync(wafer.Id,
                productDict.ContainsKey("SF-20001") ? productDict["SF-20001"].Id : Guid.Empty);
        }

        // --- 3. PERC电池片工艺路线（由硅片加工而成）---
        if (productDict.TryGetValue("SF-20003", out var cell))
        {
            await CreateCellProcessRouteAsync(cell.Id,
                productDict.ContainsKey("SF-20002") ? productDict["SF-20002"].Id : Guid.Empty);
        }

        // --- 4. 500W组件工艺路线 ---
        if (productDict.TryGetValue("FG-30001", out var module500W))
        {
            await CreateModuleProcessRouteAsync(module500W.Id, "PR-FG30001-001", "500W组件生产工艺路线",
                productDict.ContainsKey("SF-20003") ? productDict["SF-20003"].Id : Guid.Empty);
        }

        // --- 5. 550W组件工艺路线 ---
        if (productDict.TryGetValue("FG-30002", out var module550W))
        {
            await CreateModuleProcessRouteAsync(module550W.Id, "PR-FG30002-001", "550W组件生产工艺路线",
                productDict.ContainsKey("SF-20003") ? productDict["SF-20003"].Id : Guid.Empty);
        }

        // 保存所有更改
        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    /// <summary>
    /// 创建单晶硅棒工艺路线
    /// 工艺流程：配料 → 装料 → 拉晶 → 冷却 → 检测
    /// </summary>
    private async Task CreateIngotProcessRouteAsync(Guid productId)
    {
        var route = new ProcessRoute(
            Guid.NewGuid(),
            "PR-SF20001-001",
            "单晶硅棒生产工艺路线",
            productId,
            "V1.0"
        );

        // OP10: 配料工序
        var step10 = route.AddStep("OP10", "多晶硅料配料", standardTime: 30m, isCritical: false);

        // OP20: 装料工序
        var step20 = route.AddStep("OP20", "石英坩埚装料", standardTime: 45m, isCritical: true);

        // OP30: 拉晶工序（关键工序）
        var step30 = route.AddStep("OP30", "单晶拉制", standardTime: 720m, isCritical: true);
        // 配置工艺参数示例：拉晶温度、拉速等

        // OP40: 冷却工序
        var step40 = route.AddStep("OP40", "缓慢冷却", standardTime: 180m, isCritical: true);

        // OP50: 外观检测
        var step50 = route.AddStep("OP50", "硅棒外观检测", standardTime: 15m, isCritical: false,
            isInspectionRequired: true, inspectionType: InspectionType.Final);

        await _processRouteRepository.InsertAsync(route);
    }

    /// <summary>
    /// 创建硅片工艺路线
    /// 工艺流程：开方 → 切片 → 清洗 → 分选 → 包装
    /// </summary>
    private async Task CreateWaferProcessRouteAsync(Guid productId, Guid inputProductId)
    {
        var route = new ProcessRoute(
            Guid.NewGuid(),
            "PR-SF20002-001",
            "硅片切割工艺路线",
            productId,
            "V1.0"
        );

        // OP10: 开方工序
        var step10 = route.AddStep("OP10", "硅棒开方", standardTime: 60m, isCritical: true);
        // 配置一对多转换：1根硅棒 → 多个方棒
        step10.ConfigureMaterialConversion(inputProductId, productId, yieldRate: 4m); // 1根圆棒切成4个方棒

        // OP20: 切片工序（关键工序）
        var step20 = route.AddStep("OP20", "金刚线切片", standardTime: 120m, isCritical: true);
        // 配置一对多转换：1个方棒 → 1000片硅片
        step20.ConfigureMaterialConversion(productId, productId, yieldRate: 1000m);

        // OP30: 清洗工序
        var step30 = route.AddStep("OP30", "硅片清洗", standardTime: 30m, isCritical: false);

        // OP40: 厚度分选工序（光伏特有）
        var step40 = route.AddStep("OP40", "硅片厚度分选", standardTime: 20m, isCritical: false);
        step40.ConfigureAsSortingStep(
            sortingType: SortType.Thickness,
            thresholdA: 160m, // A级：厚度≤160μm
            thresholdB: 170m, // B级：厚度≤170μm
            thresholdC: 180m, // C级：厚度≤180μm
            nextStepForGradeA: null, // A/B/C级都流向下一工序
            nextStepForGradeB: null,
            nextStepForGradeC: null
        );

        // OP50: 最终检验
        var step50 = route.AddStep("OP50", "硅片终检", standardTime: 15m, isCritical: false,
            isInspectionRequired: true, inspectionType: InspectionType.Final);

        await _processRouteRepository.InsertAsync(route);
    }

    /// <summary>
    /// 创建PERC电池片工艺路线
    /// 工艺流程：制绒 → 扩散 → 刻蚀 → PECVD → 丝网印刷 → 烧结 → 测试分选
    /// </summary>
    private async Task CreateCellProcessRouteAsync(Guid productId, Guid inputProductId)
    {
        var route = new ProcessRoute(
            Guid.NewGuid(),
            "PR-SF20003-001",
            "PERC电池片工艺路线",
            productId,
            "V1.0"
        );

        // OP10: 制绒工序
        var step10 = route.AddStep("OP10", "碱制绒", standardTime: 45m, isCritical: true);
        step10.ConfigureMaterialConversion(inputProductId, productId, yieldRate: 950m); // 1000片投入，950片产出

        // OP20: 扩散工序（关键工序）
        var step20 = route.AddStep("OP20", "磷扩散", standardTime: 180m, isCritical: true);

        // OP30: 刻蚀工序
        var step30 = route.AddStep("OP30", "边缘刻蚀", standardTime: 60m, isCritical: true);

        // OP40: PECVD镀膜（关键工序）
        var step40 = route.AddStep("OP40", "PECVD减反膜", standardTime: 90m, isCritical: true);

        // OP50: 丝网印刷（关键工序）
        var step50 = route.AddStep("OP50", "丝网印刷", standardTime: 30m, isCritical: true);

        // OP60: 烧结工序（关键工序）
        var step60 = route.AddStep("OP60", "高温烧结", standardTime: 15m, isCritical: true);

        // OP70: 效率分选工序（光伏特有 - 关键工序）
        var step70 = route.AddStep("OP70", "电池片效率分选", standardTime: 20m, isCritical: true);
        step70.ConfigureAsSortingStep(
            sortingType: SortType.Efficiency,
            thresholdA: 22.5m, // A级：效率≥22.5%
            thresholdB: 21.0m, // B级：效率≥21.0%
            thresholdC: 19.5m, // C级：效率≥19.5%
            nextStepForGradeA: null, // A/B/C级都流向包装
            nextStepForGradeB: null,
            nextStepForGradeC: null
        );

        // OP80: 最终检验
        var step80 = route.AddStep("OP80", "电池片终检", standardTime: 15m, isCritical: false,
            isInspectionRequired: true, inspectionType: InspectionType.Final);

        await _processRouteRepository.InsertAsync(route);
    }

    /// <summary>
    /// 创建组件工艺路线（适用于500W和550W）
    /// 工艺流程：电池片分选 → 焊接 → 叠层 → 层压 → 装框 → 接线盒 → 固化 → 测试 → 包装
    /// </summary>
    private async Task CreateModuleProcessRouteAsync(Guid productId, string routeCode, string routeName,
        Guid inputProductId)
    {
        var route = new ProcessRoute(
            Guid.NewGuid(),
            routeCode,
            routeName,
            productId,
            "V1.0"
        );

        // OP10: 电池片分选
        var step10 = route.AddStep("OP10", "电池片功率分选", standardTime: 30m, isCritical: false);
        step10.ConfigureMaterialConversion(inputProductId, productId, yieldRate: 72m); // 72片或78片电池片

        // OP20: 焊接工序（关键工序）
        var step20 = route.AddStep("OP20", "电池串焊接", standardTime: 45m, isCritical: true);

        // OP30: 叠层工序
        var step30 = route.AddStep("OP30", "玻璃-EVA-电池串叠层", standardTime: 20m, isCritical: true);

        // OP40: 层压工序（关键工序）
        var step40 = route.AddStep("OP40", "真空层压", standardTime: 25m, isCritical: true);

        // OP50: 装框工序
        var step50 = route.AddStep("OP50", "铝边框安装", standardTime: 15m, isCritical: false);

        // OP60: 接线盒安装
        var step60 = route.AddStep("OP60", "接线盒焊接与安装", standardTime: 10m, isCritical: false);

        // OP70: 固化工序
        var step70 = route.AddStep("OP70", "硅胶固化", standardTime: 120m, isCritical: true);

        // OP80: EL测试（关键工序）
        var step80 = route.AddStep("OP80", "EL隐裂检测", standardTime: 10m, isCritical: true,
            isInspectionRequired: true, inspectionType: InspectionType.InProcess);

        // OP90: 功率测试分选（光伏特有 - 关键工序）
        var step90 = route.AddStep("OP90", "组件功率测试分选", standardTime: 15m, isCritical: true);

        // 根据组件功率设置分选阈值（500W或550W）
        decimal powerThresholdA, powerThresholdB, powerThresholdC;
        if (routeCode.Contains("30001")) // 500W组件
        {
            powerThresholdA = 500m; // A级：≥500W
            powerThresholdB = 495m; // B级：≥495W
            powerThresholdC = 490m; // C级：≥490W
        }
        else // 550W组件
        {
            powerThresholdA = 550m; // A级：≥550W
            powerThresholdB = 545m; // B级：≥545W
            powerThresholdC = 540m; // C级：≥540W
        }

        step90.ConfigureAsSortingStep(
            sortingType: SortType.Power,
            thresholdA: powerThresholdA,
            thresholdB: powerThresholdB,
            thresholdC: powerThresholdC,
            nextStepForGradeA: null, // A/B/C级都流向包装
            nextStepForGradeB: null,
            nextStepForGradeC: null
        );

        // OP100: 最终检验与包装
        var step100 = route.AddStep("OP100", "外观检验与包装", standardTime: 20m, isCritical: false,
            isInspectionRequired: true, inspectionType: InspectionType.Final);

        await _processRouteRepository.InsertAsync(route);
    }
}