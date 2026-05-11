# ABP 后端项目结构说明

## 📁 整体架构概览

```
AbpDemo.sln
├── src/
│   ├── AbpDemo.Domain.Shared          # 共享领域层（常量、枚举、DTO基础）
│   ├── AbpDemo.Domain                 # 领域层（聚合根、实体、值对象、领域服务）
│   ├── AbpDemo.Application.Contracts  # 应用契约层（DTO、接口定义、权限定义）
│   ├── AbpDemo.Application            # 应用层（应用服务、业务编排）
│   ├── AbpDemo.EntityFrameworkCore    # 基础设施层-EF Core实现
│   ├── AbpDemo.HttpApi                # HTTP API层（Controller）
│   ├── AbpDemo.HttpApi.Client         # HTTP API客户端（用于微服务调用）
│   └── AbpDemo.HttpApi.Host           # 宿主层（启动入口、配置）
├── test/
│   ├── AbpDemo.TestBase               # 测试基础
│   ├── AbpDemo.Domain.Tests           # 领域层测试
│   ├── AbpDemo.Application.Tests      # 应用层测试
│   ├── AbpDemo.EntityFrameworkCore.Tests  # EF Core测试
│   └── AbpDemo.HttpApi.Client.ConsoleTestApp  # API客户端测试
└── docs/                              # 文档
```

---

## 🔍 各层级详细说明

### 1️⃣ AbpDemo.Domain.Shared（共享领域层）

**职责**：存放所有层共享的类型定义，不依赖任何业务逻辑

**目录结构**：
```
AbpDemo.Domain.Shared/
├── Localization/                      # 多语言资源
│   ├── AbpDemo/
│   │   ├── en.json                   # 英文资源
│   │   ├── zh-Hans.json              # 中文资源
│   │   └── ...                       # 其他语言
│   └── AbpDemoResource.cs            # 本地化资源类
├── MultiTenancy/                      # 多租户配置
│   └── MultiTenancyConsts.cs         # 多租户常量
├── Enums/                             # 枚举定义（建议新增）
│   ├── ProductType.cs
│   ├── BomType.cs
│   ├── WorkOrderStatus.cs
│   ├── InspectionType.cs
│   ├── EquipmentStatus.cs
│   └── ...                           # 所有业务枚举
├── Exceptions/                        # 异常定义（建议新增）
│   └── AbpDemoDomainErrorCodes.cs    # 错误码常量
├── AbpDemo.Domain.Shared.csproj
├── AbpDemoDomainSharedModule.cs       # 模块类
└── AbpDemoDtoExtensions.cs            # DTO扩展配置
```

**关键文件说明**：
- **枚举定义**：所有业务枚举放在 `Enums/` 目录
- **错误码**：在 `AbpDemoDomainErrorCodes.cs` 中定义业务错误码
- **多语言**：支持国际化，错误消息、界面文本都从这里读取

---

### 2️⃣ AbpDemo.Domain（领域层 - 核心）

**职责**：包含业务核心逻辑，聚合根、实体、值对象、领域服务、仓储接口

**目录结构**：
```
AbpDemo.Domain/
├── Products/                          # 产品聚合
│   ├── Product.cs                    # 聚合根
│   ├── BomItem.cs                    # 子实体
│   ├── BomVersion.cs                 # 子实体
│   ├── Events/                       # 领域事件
│   │   ├── BomCreatedEvent.cs
│   │   └── BomVersionChangedEvent.cs
│   └── Specifications/               # 规格模式（可选）
│       └── ProductByNameSpec.cs
├── Processes/                         # 工艺路线聚合
│   ├── ProcessRoute.cs               # 聚合根
│   ├── ProcessStep.cs                # 子实体
│   ├── ProcessParameter.cs           # 子实体
│   └── ProcessDocument.cs            # 子实体
├── WorkOrders/                        # 工单聚合
│   ├── WorkOrder.cs                  # 聚合根
│   ├── WorkOrderRouting.cs           # 子实体
│   ├── WorkReport.cs                 # 子实体
│   └── Events/
│       ├── WorkOrderReleasedEvent.cs
│       └── WorkOrderCompletedEvent.cs
├── Materials/                         # 物料批次聚合
│   ├── MaterialLot.cs                # 聚合根
│   └── MaterialTransaction.cs        # 子实体
├── Quality/                           # 质量管理聚合
│   ├── QualityInspection.cs          # 聚合根
│   ├── InspectionItem.cs             # 子实体
│   └── NonConformingProduct.cs       # 聚合根
├── Equipment/                         # 设备管理聚合
│   ├── Equipment.cs                  # 聚合根
│   ├── EquipmentMaintenance.cs       # 子实体
│   └── EquipmentDataLog.cs           # 实体（非聚合根）
├── CleanRooms/                        # 洁净车间聚合
│   ├── CleanRoom.cs                  # 聚合根
│   ├── EnvironmentMonitor.cs         # 子实体
│   └── CleanRoomAccess.cs            # 子实体
├── Shared/                            # 共享值对象和领域服务
│   ├── ValueObjects/
│   │   ├── ProductSpecification.cs
│   │   ├── ParameterRange.cs
│   │   ├── NetworkAddress.cs
│   │   └── EnvironmentParameterRange.cs
│   └── DomainServices/
│       ├── IBomConversionService.cs
│       ├── IWorkOrderSchedulingService.cs
│       ├── IMaterialLotService.cs
│       ├── IQualityInspectionService.cs
│       ├── IEquipmentDataService.cs
│       └── ICleanRoomService.cs
├── Data/                              # 数据迁移相关
│   ├── IAbpDemoDbSchemaMigrator.cs
│   ├── AbpDemoDbMigrationService.cs
│   └── NullAbpDemoDbSchemaMigrator.cs
├── Settings/                          # 设置定义
│   ├── AbpDemoSettings.cs
│   └── AbpDemoSettingDefinitionProvider.cs
├── OpenIddict/                        # OpenIddict配置
│   └── OpenIddictDataSeedContributor.cs
├── AbpDemo.Domain.csproj
├── AbpDemoConsts.cs                   # 全局常量
├── AbpDemoDomainModule.cs             # 模块类
└── AssemblyInfo.cs
```

**关键设计原则**：
- ✅ **聚合根**继承 `AggregateRoot<Guid>` 或 `FullAuditedAggregateRoot<Guid>`
- ✅ **子实体**继承 `Entity<Guid>`
- ✅ **值对象**继承 `ValueObject`
- ✅ **仓储接口**以 `I` 开头，如 `IProductRepository`
- ✅ **领域服务接口**以 `I` 开头，放在 `DomainServices/` 目录
- ✅ **领域事件**放在各聚合的 `Events/` 子目录
- ❌ **不包含**任何持久化逻辑、HTTP相关代码

---

### 3️⃣ AbpDemo.Application.Contracts（应用契约层）

**职责**：定义应用服务接口、DTO、权限常量，供应用层和API层共享

**目录结构**：
```
AbpDemo.Application.Contracts/
├── Products/                          # 产品相关DTO和接口
│   ├── IProductAppService.cs         # 应用服务接口
│   ├── Dtos/
│   │   ├── ProductDto.cs
│   │   ├── CreateProductRequest.cs
│   │   ├── UpdateProductRequest.cs
│   │   ├── ProductWithBomDto.cs      # 包含BOM的DTO
│   │   └── BomItemDto.cs
│   └── Permissions/
│       └── ProductPermissions.cs     # 权限常量
├── Processes/                         # 工艺路线相关
│   ├── IProcessRouteAppService.cs
│   └── Dtos/
│       ├── ProcessRouteDto.cs
│       ├── CreateProcessRouteRequest.cs
│       ├── ProcessStepDto.cs
│       └── ProcessParameterDto.cs
├── WorkOrders/                        # 工单相关
│   ├── IWorkOrderAppService.cs
│   └── Dtos/
│       ├── WorkOrderDto.cs
│       ├── CreateWorkOrderRequest.cs
│       ├── ReleaseWorkOrderRequest.cs
│       ├── WorkOrderRoutingDto.cs
│       └── WorkReportDto.cs
├── Materials/                         # 物料相关
│   ├── IMaterialLotAppService.cs
│   └── Dtos/
│       ├── MaterialLotDto.cs
│       ├── MaterialTransactionDto.cs
│       └── IssueMaterialRequest.cs
├── Quality/                           # 质量相关
│   ├── IQualityInspectionAppService.cs
│   ├── INonConformingProductAppService.cs
│   └── Dtos/
│       ├── QualityInspectionDto.cs
│       ├── InspectionItemDto.cs
│       └── NonConformingProductDto.cs
├── Equipment/                         # 设备相关
│   ├── IEquipmentAppService.cs
│   └── Dtos/
│       ├── EquipmentDto.cs
│       ├── EquipmentMaintenanceDto.cs
│       └── EquipmentDataLogDto.cs
├── CleanRooms/                        # 洁净车间相关
│   ├── ICleanRoomAppService.cs
│   └── Dtos/
│       ├── CleanRoomDto.cs
│       ├── EnvironmentMonitorDto.cs
│       └── CleanRoomAccessDto.cs
├── Permissions/                       # 权限定义
│   ├── AbpDemoPermissions.cs         # 所有权限常量
│   └── AbpDemoPermissionDefinitionProvider.cs
├── AbpDemo.Application.Contracts.csproj
├── AbpDemoApplicationContractsModule.cs
└── AbpDemoDtoExtensions.cs
```

**DTO命名规范**：
- **查询返回**：`XxxDto`（如 `ProductDto`）
- **创建请求**：`CreateXxxRequest` 或 `CreateXxxInput`
- **更新请求**：`UpdateXxxRequest` 或 `UpdateXxxInput`
- **复杂查询**：`GetXxxListInput`（包含分页、筛选条件）
- **特殊场景**：`XxxWithYyyDto`（如 `ProductWithBomDto`）

**权限定义示例**：
```csharp
public static class AbpDemoPermissions
{
    public const string GroupName = "AbpDemo";
    
    public static class Products
    {
        public const string Default = GroupName + ".Products";
        public const string Create = Default + ".Create";
        public const string Update = Default + ".Update";
        public const string Delete = Default + ".Delete";
    }
    
    public static class WorkOrders
    {
        public const string Default = GroupName + ".WorkOrders";
        public const string Create = Default + ".Create";
        public const string Release = Default + ".Release";
        public const string Cancel = Default + ".Cancel";
    }
}
```

---

### 4️⃣ AbpDemo.Application（应用层）

**职责**：实现应用服务，协调领域对象完成业务用例，处理DTO转换

**目录结构**：
```
AbpDemo.Application/
├── Products/                          # 产品应用服务
│   ├── ProductAppService.cs          # 实现 IProductAppService
│   └── Mappers/
│       └── ProductMapper.cs          # AutoMapper配置
├── Processes/                         # 工艺路线应用服务
│   └── ProcessRouteAppService.cs
├── WorkOrders/                        # 工单应用服务
│   ├── WorkOrderAppService.cs
│   ├── WorkOrderSchedulingService.cs # 应用服务（协调多个聚合）
│   └── Mappers/
│       └── WorkOrderMapper.cs
├── Materials/                         # 物料应用服务
│   ├── MaterialLotAppService.cs
│   └── MaterialTransactionAppService.cs
├── Quality/                           # 质量应用服务
│   ├── QualityInspectionAppService.cs
│   └── NonConformingProductAppService.cs
├── Equipment/                         # 设备应用服务
│   ├── EquipmentAppService.cs
│   └── EquipmentMonitoringService.cs # 设备监控应用服务
├── CleanRooms/                        # 洁净车间应用服务
│   └── CleanRoomAppService.cs
├── Integration/                       # 外部系统集成（预留）
│   ├── ErpIntegrationService.cs
│   ├── PdmIntegrationService.cs
│   └── ScadaIntegrationService.cs
├── EventHandlers/                     # 领域事件处理器
│   ├── WorkOrderReleasedEventHandler.cs
│   ├── WorkOrderCompletedEventHandler.cs
│   ├── NonConformingCreatedEventHandler.cs
│   └── EquipmentBreakdownEventHandler.cs
├── Jobs/                              # 后台作业（可选）
│   ├── CalculateOeeJob.cs            # OEE计算作业
│   └── CleanOldDataJob.cs            # 清理旧数据作业
├── AbpDemo.Application.csproj
├── AbpDemoAppService.cs              # 应用服务基类
├── AbpDemoApplicationAutoMapperProfile.cs  # AutoMapper配置
└── AbpDemoApplicationModule.cs
```

**应用服务基类**：
```csharp
public abstract class AbpDemoAppService : ApplicationService
{
    // 所有应用服务继承此类
    // 可以添加通用属性：CurrentUser, ObjectMapper, 等
}
```

**应用服务示例**：
```csharp
public class WorkOrderAppService : AbpDemoAppService, IWorkOrderAppService
{
    private readonly IWorkOrderRepository _workOrderRepository;
    private readonly IProductRepository _productRepository;
    private readonly IProcessRouteRepository _processRouteRepository;
    private readonly IMaterialLotService _materialLotService; // 领域服务
    
    public async Task<WorkOrderDto> CreateAsync(CreateWorkOrderRequest input)
    {
        // 1. 验证输入
        var product = await _productRepository.GetAsync(input.ProductId);
        var processRoute = await _processRouteRepository.GetAsync(input.ProcessRouteId);
        
        // 2. 创建领域对象
        var workOrder = new WorkOrder(
            GuidGenerator.Create(),
            input.OrderNo,
            input.ProductId,
            input.BomVersionId,
            input.ProcessRouteId,
            input.Quantity
        );
        
        // 3. 保存
        await _workOrderRepository.InsertAsync(workOrder);
        
        // 4. 返回DTO
        return ObjectMapper.Map<WorkOrder, WorkOrderDto>(workOrder);
    }
    
    public async Task ReleaseAsync(Guid id, ReleaseWorkOrderRequest input)
    {
        var workOrder = await _workOrderRepository.GetAsync(id);
        
        // 调用领域方法，内部会发布 WorkOrderReleasedEvent
        workOrder.Release(CurrentUser.Id ?? throw new UserFriendlyException("用户未登录"), 
                         DateTime.UtcNow);
        
        await _workOrderRepository.UpdateAsync(workOrder);
        
        // 领域事件处理器会自动执行物料预留等操作
    }
}
```

---

### 5️⃣ AbpDemo.EntityFrameworkCore（基础设施层）

**职责**：EF Core实现，包括DbContext、实体映射、仓储实现

**目录结构**：
```
AbpDemo.EntityFrameworkCore/
├── EntityFrameworkCore/
│   ├── AbpDemoDbContext.cs           # DbContext
│   ├── AbpDemoDbContextFactory.cs    # 设计时工厂（用于迁移）
│   ├── AbpDemoEntityFrameworkCoreModule.cs  # 模块类
│   └── AbpDemoEfCoreEntityExtensionMappings.cs  # 实体扩展映射
├── EntityConfigurations/              # 实体映射配置（建议新增）
│   ├── ProductConfiguration.cs
│   ├── BomItemConfiguration.cs
│   ├── BomVersionConfiguration.cs
│   ├── ProcessRouteConfiguration.cs
│   ├── ProcessStepConfiguration.cs
│   ├── WorkOrderConfiguration.cs
│   ├── WorkOrderRoutingConfiguration.cs
│   ├── WorkReportConfiguration.cs
│   ├── MaterialLotConfiguration.cs
│   ├── MaterialTransactionConfiguration.cs
│   ├── QualityInspectionConfiguration.cs
│   ├── InspectionItemConfiguration.cs
│   ├── NonConformingProductConfiguration.cs
│   ├── EquipmentConfiguration.cs
│   ├── EquipmentMaintenanceConfiguration.cs
│   ├── EquipmentDataLogConfiguration.cs
│   ├── CleanRoomConfiguration.cs
│   ├── EnvironmentMonitorConfiguration.cs
│   └── CleanRoomAccessConfiguration.cs
├── Repositories/                      # 自定义仓储实现（建议新增）
│   ├── ProductRepository.cs
│   ├── WorkOrderRepository.cs
│   ├── MaterialLotRepository.cs
│   ├── QualityInspectionRepository.cs
│   └── EquipmentRepository.cs
├── Migrations/                        # 数据库迁移文件（自动生成）
│   ├── 20260509000000_Initial.cs
│   ├── 20260509000000_Initial.Designer.cs
│   └── AbpDemoDbContextModelSnapshot.cs
└── AbpDemo.EntityFrameworkCore.csproj
```

**DbContext示例**：
```csharp
[ConnectionStringName("Default")]
public class AbpDemoDbContext : AbpDbContext<AbpDemoDbContext>
{
    // Products
    public DbSet<Product> Products => Set<Product>();
    public DbSet<BomItem> BomItems => Set<BomItem>();
    public DbSet<BomVersion> BomVersions => Set<BomVersion>();
    
    // Processes
    public DbSet<ProcessRoute> ProcessRoutes => Set<ProcessRoute>();
    public DbSet<ProcessStep> ProcessSteps => Set<ProcessStep>();
    
    // WorkOrders
    public DbSet<WorkOrder> WorkOrders => Set<WorkOrder>();
    public DbSet<WorkOrderRouting> WorkOrderRoutings => Set<WorkOrderRouting>();
    public DbSet<WorkReport> WorkReports => Set<WorkReport>();
    
    // Materials
    public DbSet<MaterialLot> MaterialLots => Set<MaterialLot>();
    public DbSet<MaterialTransaction> MaterialTransactions => Set<MaterialTransaction>();
    
    // Quality
    public DbSet<QualityInspection> QualityInspections => Set<QualityInspection>();
    public DbSet<InspectionItem> InspectionItems => Set<InspectionItem>();
    public DbSet<NonConformingProduct> NonConformingProducts => Set<NonConformingProduct>();
    
    // Equipment
    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<EquipmentMaintenance> EquipmentMaintenances => Set<EquipmentMaintenance>();
    public DbSet<EquipmentDataLog> EquipmentDataLogs => Set<EquipmentDataLog>();
    
    // CleanRooms
    public DbSet<CleanRoom> CleanRooms => Set<CleanRoom>();
    public DbSet<EnvironmentMonitor> EnvironmentMonitors => Set<EnvironmentMonitor>();
    public DbSet<CleanRoomAccess> CleanRoomAccesses => Set<CleanRoomAccesses>();
    
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        
        // 应用所有配置
        builder.ApplyConfigurationsFromAssembly(typeof(AbpDemoEntityFrameworkCoreModule).Assembly);
        
        // 配置软删除全局过滤
        builder.Entity<Product>().HasQueryFilter(e => !e.IsDeleted);
        builder.Entity<WorkOrder>().HasQueryFilter(e => !e.IsDeleted);
        // ... 其他需要软删除的实体
    }
}
```

**实体配置示例**：
```csharp
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable(AbpDemoConsts.DbTablePrefix + "Products", AbpDemoConsts.DbSchema);
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.ProductCode)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(p => p.ProductCode)
            .IsUnique();
        
        builder.Property(p => p.ProductName)
            .IsRequired()
            .HasMaxLength(200);
        
        // 配置 owned entity（值对象）
        builder.OwnsOne(p => p.Specification, spec =>
        {
            spec.Property(s => s.Length).HasColumnName("Specification_Length");
            spec.Property(s => s.Width).HasColumnName("Specification_Width");
            spec.Property(s => s.Height).HasColumnName("Specification_Height");
        });
        
        // 配置导航属性
        builder.HasMany(p => p.BomItems)
            .WithOne()
            .HasForeignKey(b => b.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasMany(p => p.Versions)
            .WithOne()
            .HasForeignKey(v => v.ProductId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**自定义仓储示例**：
```csharp
public class ProductRepository : EfCoreRepository<AbpDemoDbContext, Product, Guid>, 
                                  IProductRepository
{
    public ProductRepository(IDbContextProvider<AbpDemoDbContext> dbContextProvider) 
        : base(dbContextProvider)
    {
    }
    
    public async Task<Product> GetWithBomItemsAsync(Guid id)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet
            .Include(p => p.BomItems)
            .Include(p => p.Versions)
            .FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Product> FindByCodeAsync(string productCode)
    {
        var dbSet = await GetDbSetAsync();
        return await dbSet.FirstOrDefaultAsync(p => p.ProductCode == productCode);
    }
}
```

---

### 6️⃣ AbpDemo.HttpApi（API层）

**职责**：提供RESTful API端点，通常由ABP框架自动生成Controller

**目录结构**：
```
AbpDemo.HttpApi/
├── Controllers/
│   └── AbpDemoController.cs          # Controller基类
├── Models/
│   └── Test/
│       └── TestModel.cs
├── AbpDemo.HttpApi.csproj
└── AbpDemoHttpApiModule.cs
```

**说明**：
- ABP框架会根据 `Application.Contracts` 中的接口自动生成Controller
- 不需要手动编写Controller代码（除非有特殊需求）
- API路由规则：`/api/app/{service-name}/{action}`

---

### 7️⃣ AbpDemo.HttpApi.Client（API客户端）

**职责**：提供HTTP API客户端代理，用于微服务间调用或前端SDK

**目录结构**：
```
AbpDemo.HttpApi.Client/
├── AbpDemo.HttpApi.Client.csproj
└── AbpDemoHttpApiClientModule.cs
```

**使用场景**：
- 微服务架构中，其他服务通过此客户端调用本服务
- 为前端生成TypeScript客户端代码

---

### 8️⃣ AbpDemo.HttpApi.Host（宿主层）

**职责**：应用程序入口，配置中间件、依赖注入、数据库连接等

**目录结构**：
```
AbpDemo.HttpApi.Host/
├── Controllers/
│   └── HomeController.cs             # 默认首页Controller
├── Properties/
│   └── launchSettings.json           # 启动配置
├── wwwroot/                          # 静态资源
│   ├── images/logo/leptonx/
│   ├── libs/
│   └── global-styles.css
├── Logs/                             # 日志文件
├── Program.cs                        # 程序入口
├── AbpDemoBrandingProvider.cs        # 品牌配置
├── AbpDemoHttpApiHostModule.cs       # 宿主模块
├── appsettings.json                  # 配置文件
├── appsettings.Development.json      # 开发环境配置
├── appsettings.secrets.json          # 机密配置（不提交到Git）
├── package.json                      # NPM包配置
├── abp.resourcemapping.js            # 资源映射
└── web.config
```

**Program.cs示例**：
```csharp
await CreateHostBuilder(args).RunAsync();

static IHostBuilder CreateHostBuilder(string[] args) =>
    Host.CreateDefaultBuilder(args)
        .AddAppSettingsSecretsJson()
        .ConfigureLogging((context, logging) =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.AddFile("Logs/app.log");
        })
        .ConfigureWebHostDefaults(webBuilder =>
        {
            webBuilder.UseStartup<Startup>();
        });
```

**appsettings.json关键配置**：
```json
{
  "ConnectionStrings": {
    "Default": "Server=localhost;Database=AbpDemo;Trusted_Connection=True;TrustServerCertificate=true;"
  },
  "AuthServer": {
    "Authority": "https://localhost:44301",
    "RequireHttpsMetadata": true
  },
  "StringEncryption": {
    "DefaultPassPhrase": "YOUR_PASSPHRASE_HERE"
  }
}
```

---

## 🎯 依赖关系图

```
┌─────────────────────────────────────┐
│   AbpDemo.HttpApi.Host (宿主层)     │
│   - 启动配置、中间件、依赖注入       │
└──────────────┬──────────────────────┘
               │ 引用
┌──────────────▼──────────────────────┐
│   AbpDemo.HttpApi (API层)           │
│   - Controller基类                  │
└──────────────┬──────────────────────┘
               │ 引用
┌──────────────▼──────────────────────┐
│   AbpDemo.Application (应用层)      │
│   - 应用服务实现                     │
│   - 事件处理器                       │
│   - 后台作业                         │
└──────────────┬──────────────────────┘
               │ 引用
┌──────────────▼──────────────────────┐
│ AbpDemo.Application.Contracts       │
│   - 应用服务接口                     │
│   - DTO定义                         │
│   - 权限常量                         │
└──────────────┬──────────────────────┘
               │ 引用
┌──────────────▼──────────────────────┐
│   AbpDemo.Domain (领域层)           │
│   - 聚合根、实体、值对象             │
│   - 领域服务接口                     │
│   - 仓储接口                         │
│   - 领域事件                         │
└──────────────┬──────────────────────┘
               │ 引用
┌──────────────▼──────────────────────┐
│ AbpDemo.Domain.Shared (共享层)      │
│   - 枚举、常量                       │
│   - 异常定义                         │
│   - 多语言资源                       │
└─────────────────────────────────────┘

┌─────────────────────────────────────┐
│ AbpDemo.EntityFrameworkCore         │
│   - DbContext实现                   │
│   - 实体映射配置                     │
│   - 仓储实现                         │
└──────────────┬──────────────────────┘
               │ 引用 Domain 层
```

**依赖规则**：
- ✅ 上层可以引用下层
- ❌ 下层不能引用上层（严格分层）
- ❌ 同层之间尽量避免循环依赖
- ✅ Domain层不依赖任何基础设施（EF Core、HTTP等）

---

## 📦 NuGet包依赖关系

| 项目 | 主要NuGet包依赖 |
|------|----------------|
| **Domain.Shared** | Volo.Abp.Core, Volo.Abp.Validation |
| **Domain** | AbpDemo.Domain.Shared, Volo.Abp.Ddd.Domain |
| **Application.Contracts** | AbpDemo.Domain.Shared, Volo.Abp.Ddd.Application.Contracts |
| **Application** | AbpDemo.Application.Contracts, AbpDemo.Domain, Volo.Abp.AutoMapper |
| **EntityFrameworkCore** | AbpDemo.Domain, Volo.Abp.EntityFrameworkCore.SqlServer |
| **HttpApi** | AbpDemo.Application.Contracts, Volo.Abp.AspNetCore.Mvc |
| **HttpApi.Client** | AbpDemo.Application.Contracts, Volo.Abp.Http.Client |
| **HttpApi.Host** | 所有层 + Volo.Abp.AspNetCore.Serilog + Volo.Abp.Swashbuckle |

---

## 🔧 常用开发命令

### 数据库迁移
```bash
# 添加迁移
cd src/AbpDemo.DbMigrator
dotnet ef migrations add Initial -c AbpDemoDbContext -p ../AbpDemo.EntityFrameworkCore/AbpDemo.EntityFrameworkCore.csproj

# 更新数据库
dotnet ef database update -c AbpDemoDbContext -p ../AbpDemo.EntityFrameworkCore/AbpDemo.EntityFrameworkCore.csproj

# 或使用DbMigrator项目
cd src/AbpDemo.DbMigrator
dotnet run
```

### 运行项目
```bash
# 运行API宿主
cd src/AbpDemo.HttpApi.Host
dotnet run

# 运行数据库迁移器
cd src/AbpDemo.DbMigrator
dotnet run
```

### 运行测试
```bash
# 运行所有测试
dotnet test

# 运行特定测试项目
dotnet test test/AbpDemo.Application.Tests/AbpDemo.Application.Tests.csproj
```

---

## 📝 开发最佳实践

### 1. 命名规范
- **聚合根**：名词，如 `Product`, `WorkOrder`
- **领域服务**：动词+名词，如 `BomConversionService`
- **应用服务**：名词+AppService，如 `ProductAppService`
- **DTO**：`XxxDto`, `CreateXxxRequest`, `UpdateXxxRequest`
- **仓储接口**：`IXxxRepository`
- **事件**：`XxxEvent` 或 `XxxChangedEvent`

### 2. 文件组织
- 按**业务领域**分组，不按技术类型分组
- 每个领域一个文件夹，包含该领域的所有相关文件
- 领域事件放在 `Events/` 子目录
- DTO放在 `Dtos/` 子目录

### 3. 依赖注入
- 构造函数注入是首选方式
- 使用接口而非具体实现
- ABP自动注册服务（约定优于配置）

### 4. 异常处理
- 使用 `BusinessException` 抛出业务异常
- 在 `AbpDemoDomainErrorCodes.cs` 中定义错误码
- 异常消息通过本地化资源提供多语言支持

### 5. 事务管理
- 应用服务方法默认包裹在工作单元（UOW）中
- 跨聚合操作使用同一UOW保证原子性
- 需要手动控制事务时使用 `IUnitOfWorkManager`

---

## 🚀 下一步行动

1. **创建枚举文件**：在 `Domain.Shared/Enums/` 中创建所有业务枚举
2. **定义错误码**：在 `Domain.Shared/Exceptions/AbpDemoDomainErrorCodes.cs` 中定义错误码
3. **创建聚合根**：从核心聚合开始（Product, WorkOrder, MaterialLot）
4. **定义仓储接口**：为每个聚合根创建仓储接口
5. **配置实体映射**：在 `EntityConfigurations/` 中创建EF Core配置
6. **实现应用服务**：实现CRUD操作和业务逻辑
7. **编写单元测试**：为领域模型和应用服务编写测试

---

**文档版本**：V1.0  
**最后更新**：2026-05-09  
**适用框架**：ABP Framework 8.x (.NET 8)
