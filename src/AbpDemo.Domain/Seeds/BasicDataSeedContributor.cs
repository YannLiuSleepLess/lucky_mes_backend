using System;
using System.Threading.Tasks;
using AbpDemo.BasicData.WorkCenters.Aggregates;
using AbpDemo.BasicData.Workshops.Aggregates;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;

namespace AbpDemo.Domain.Seeds;

/// <summary>
/// 基础数据种子贡献者
/// 初始化车间和工作中心数据
/// </summary>
public class BasicDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly IRepository<Workshop, Guid> _workshopRepository;
    private readonly IRepository<WorkCenter, Guid> _workCenterRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public BasicDataSeedContributor(
        IRepository<Workshop, Guid> workshopRepository,
        IRepository<WorkCenter, Guid> workCenterRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _workshopRepository = workshopRepository;
        _workCenterRepository = workCenterRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        if (await _workshopRepository.CountAsync() > 0)
        {
            return;
        }

        // ========================================
        // 第一步：创建车间
        // ========================================

        // --- 拉晶车间 ---
        var crystalWorkshop = await CreateWorkshopAsync("WS-001", "拉晶车间", "A栋1楼");

        // --- 切片车间 ---
        var waferWorkshop = await CreateWorkshopAsync("WS-002", "切片车间", "A栋2楼");

        // --- 电池车间 ---
        var cellWorkshop = await CreateWorkshopAsync("WS-003", "电池车间", "B栋1楼");

        // --- 组件车间 ---
        var moduleWorkshop = await CreateWorkshopAsync("WS-004", "组件车间", "C栋1楼");

        // --- 质检车间 ---
        var qualityWorkshop = await CreateWorkshopAsync("WS-005", "质检车间", "D栋1楼");

        // --- 仓储车间 ---
        var warehouseWorkshop = await CreateWorkshopAsync("WS-006", "仓储车间", "E栋1楼");

        await _unitOfWorkManager.Current.SaveChangesAsync();

        // ========================================
        // 第二步：创建工作中心
        // ========================================

        // --- 拉晶车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS001-01", "配料工作中心", crystalWorkshop.Id, capacity: 60, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS001-02", "装料工作中心", crystalWorkshop.Id, capacity: 24, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS001-03", "拉晶工作中心", crystalWorkshop.Id, capacity: 12, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS001-04", "冷却工作中心", crystalWorkshop.Id, capacity: 48, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS001-05", "单晶检测工作中心", crystalWorkshop.Id, capacity: 100, shiftCount: 2);

        // --- 切片车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS002-01", "开方工作中心", waferWorkshop.Id, capacity: 30, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS002-02", "切片工作中心", waferWorkshop.Id, capacity: 20, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS002-03", "清洗工作中心", waferWorkshop.Id, capacity: 50, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS002-04", "厚度分选工作中心", waferWorkshop.Id, capacity: 80, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS002-05", "硅片终检工作中心", waferWorkshop.Id, capacity: 100, shiftCount: 2);

        // --- 电池车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS003-01", "制绒工作中心", cellWorkshop.Id, capacity: 60, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-02", "扩散工作中心", cellWorkshop.Id, capacity: 40, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-03", "刻蚀工作中心", cellWorkshop.Id, capacity: 50, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-04", "PECVD工作中心", cellWorkshop.Id, capacity: 30, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-05", "丝网印刷工作中心", cellWorkshop.Id, capacity: 45, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-06", "烧结工作中心", cellWorkshop.Id, capacity: 50, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS003-07", "效率分选工作中心", cellWorkshop.Id, capacity: 80, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS003-08", "电池终检工作中心", cellWorkshop.Id, capacity: 100, shiftCount: 2);

        // --- 组件车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS004-01", "电池分选工作中心", moduleWorkshop.Id, capacity: 80, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS004-02", "焊接工作中心", moduleWorkshop.Id, capacity: 30, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-03", "叠层工作中心", moduleWorkshop.Id, capacity: 25, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-04", "层压工作中心", moduleWorkshop.Id, capacity: 20, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-05", "装框工作中心", moduleWorkshop.Id, capacity: 40, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-06", "接线盒安装工作中心", moduleWorkshop.Id, capacity: 50, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-07", "固化工作中心", moduleWorkshop.Id, capacity: 30, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS004-08", "EL检测工作中心", moduleWorkshop.Id, capacity: 60, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS004-09", "功率分选工作中心", moduleWorkshop.Id, capacity: 80, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS004-10", "包装工作中心", moduleWorkshop.Id, capacity: 50, shiftCount: 2);

        // --- 质检车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS005-01", "来料检验工作中心", qualityWorkshop.Id, capacity: 200, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS005-02", "过程检验工作中心", qualityWorkshop.Id, capacity: 150, shiftCount: 3);
        await CreateWorkCenterAsync("WC-WS005-03", "成品检验工作中心", qualityWorkshop.Id, capacity: 100, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS005-04", "可靠性测试工作中心", qualityWorkshop.Id, capacity: 30, shiftCount: 1);

        // --- 仓储车间工作中心 ---
        await CreateWorkCenterAsync("WC-WS006-01", "原材料仓工作中心", warehouseWorkshop.Id, capacity: 500, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS006-02", "半成品仓工作中心", warehouseWorkshop.Id, capacity: 300, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS006-03", "成品仓工作中心", warehouseWorkshop.Id, capacity: 200, shiftCount: 2);
        await CreateWorkCenterAsync("WC-WS006-04", "发货工作中心", warehouseWorkshop.Id, capacity: 100, shiftCount: 2);

        await _unitOfWorkManager.Current.SaveChangesAsync();
    }

    private async Task<Workshop> CreateWorkshopAsync(string code, string name, string location)
    {
        var workshop = new Workshop(Guid.NewGuid(), code, name);
        workshop.SetLocation(location);
        await _workshopRepository.InsertAsync(workshop);
        return workshop;
    }

    private async Task<WorkCenter> CreateWorkCenterAsync(
        string code, string name, Guid workshopId, int capacity, int shiftCount)
    {
        var workCenter = new WorkCenter(Guid.NewGuid(), code, name, workshopId);
        workCenter.SetCapacity(capacity);
        workCenter.SetShiftCount(shiftCount);
        await _workCenterRepository.InsertAsync(workCenter);
        return workCenter;
    }
}