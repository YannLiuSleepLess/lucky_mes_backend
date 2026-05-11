using AbpDemo.Engineering.Changes;
using AbpDemo.Engineering.Processes;
using AbpDemo.Engineering.Products;
using Microsoft.EntityFrameworkCore;
using Volo.Abp.AuditLogging.EntityFrameworkCore;
using Volo.Abp.BackgroundJobs.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.FeatureManagement.EntityFrameworkCore;
using Volo.Abp.Identity;
using Volo.Abp.Identity.EntityFrameworkCore;
using Volo.Abp.OpenIddict.EntityFrameworkCore;
using Volo.Abp.PermissionManagement.EntityFrameworkCore;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Volo.Abp.TenantManagement;
using Volo.Abp.TenantManagement.EntityFrameworkCore;

namespace AbpDemo.EntityFrameworkCore;

[ReplaceDbContext(typeof(IIdentityDbContext))]
[ReplaceDbContext(typeof(ITenantManagementDbContext))]
[ConnectionStringName("Default")]
public class AbpDemoDbContext :
    AbpDbContext<AbpDemoDbContext>,
    IIdentityDbContext,
    ITenantManagementDbContext
{
    /* Add DbSet properties for your Aggregate Roots / Entities here. */
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVersion> ProductVersions { get; set; }
    public DbSet<BomItem> BomItems { get; set; }
    public DbSet<ProcessRoute> ProcessRoutes { get; set; }
    public DbSet<ProcessStep> ProcessSteps { get; set; }
    public DbSet<ProcessParameter> ProcessParameters { get; set; }
    public DbSet<ProcessDocument> ProcessDocuments { get; set; }
    public DbSet<EngineeringChange> EngineeringChanges { get; set; }

    #region Entities from the modules

    /* Notice: We only implemented IIdentityDbContext and ITenantManagementDbContext
     * and replaced them for this DbContext. This allows you to perform JOIN
     * queries for the entities of these modules over the repositories easily. You
     * typically don't need that for other modules. But, if you need, you can
     * implement the DbContext interface of the needed module and use ReplaceDbContext
     * attribute just like IIdentityDbContext and ITenantManagementDbContext.
     *
     * More info: Replacing a DbContext of a module ensures that the related module
     * uses this DbContext on runtime. Otherwise, it will use its own DbContext class.
     */

    //Identity
    public DbSet<IdentityUser> Users { get; set; }
    public DbSet<IdentityRole> Roles { get; set; }
    public DbSet<IdentityClaimType> ClaimTypes { get; set; }
    public DbSet<OrganizationUnit> OrganizationUnits { get; set; }
    public DbSet<IdentitySecurityLog> SecurityLogs { get; set; }
    public DbSet<IdentityLinkUser> LinkUsers { get; set; }
    public DbSet<IdentityUserDelegation> UserDelegations { get; set; }

    public DbSet<IdentitySession> Sessions { get; set; }

    // Tenant Management
    public DbSet<Tenant> Tenants { get; set; }
    public DbSet<TenantConnectionString> TenantConnectionStrings { get; set; }

    #endregion

    public AbpDemoDbContext(DbContextOptions<AbpDemoDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        /* Include modules to your migration db context */

        builder.ConfigurePermissionManagement();
        builder.ConfigureSettingManagement();
        builder.ConfigureBackgroundJobs();
        builder.ConfigureAuditLogging();
        builder.ConfigureIdentity();
        builder.ConfigureOpenIddict();
        builder.ConfigureFeatureManagement();
        builder.ConfigureTenantManagement();

        /* Configure your own tables/entities inside here */

        builder.Entity<Product>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "Products", AbpDemoConsts.DbSchema);
            b.Property(x => x.ProductCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
            b.HasIndex(x => x.ProductCode).IsUnique();

            // 光伏特有字段
            b.Property(x => x.IsSerialTraced).HasColumnName("IsSerialTraced").HasDefaultValue(false);
            b.Property(x => x.YieldRate).HasColumnName("YieldRate");

            // 配置 ProductSpecification 为 Owned Entity（值对象）
            b.OwnsOne(p => p.Specification, spec =>
            {
                spec.Property(s => s.Length).HasColumnName("SpecLength");
                spec.Property(s => s.Width).HasColumnName("SpecWidth");
                spec.Property(s => s.Height).HasColumnName("SpecHeight");
                spec.Property(s => s.Thickness).HasColumnName("SpecThickness");
                spec.Property(s => s.Weight).HasColumnName("SpecWeight");
                spec.Property(s => s.CustomSpecs).HasColumnName("SpecCustomSpecs").HasMaxLength(500);
            });

            // 配置 Product 与 ProductVersion 的一对多关系
            b.HasMany(p => p.Versions)
                .WithOne()
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductVersion>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "ProductVersions", AbpDemoConsts.DbSchema);
            b.Property(x => x.VersionNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.ChangeReason).HasMaxLength(500);

            // 配置 ProductVersion 与 BomItem 的一对多关系
            b.HasMany(v => v.BomItems)
                .WithOne()
                .HasForeignKey(i => i.ProductVersionId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<BomItem>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "BomItems", AbpDemoConsts.DbSchema);
            b.Property(x => x.Unit).HasMaxLength(20);

            // 光伏特有字段
            b.Property(x => x.YieldRate).HasColumnName("YieldRate");
        });

        builder.Entity<ProcessRoute>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "ProcessRoutes", AbpDemoConsts.DbSchema);
            b.Property(x => x.RouteCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.RouteName).IsRequired().HasMaxLength(200);
            b.Property(x => x.Version).IsRequired().HasMaxLength(20);
            b.HasIndex(x => x.RouteCode).IsUnique();

            // 配置一对多关系
            b.HasMany(p => p.Steps)
                .WithOne()
                .HasForeignKey(s => s.ProcessRouteId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Parameters)
                .WithOne()
                .HasForeignKey(p => p.ProcessStepId)
                .OnDelete(DeleteBehavior.Cascade);

            b.HasMany(p => p.Documents)
                .WithOne()
                .HasForeignKey(d => d.ProcessRouteId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProcessStep>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "ProcessSteps", AbpDemoConsts.DbSchema);
            b.Property(x => x.StepNo).IsRequired().HasMaxLength(20);
            b.Property(x => x.StepName).IsRequired().HasMaxLength(100);

            // 光伏特有字段
            b.Property(x => x.IsSortingStep).HasColumnName("IsSortingStep").HasDefaultValue(false);
            b.Property(x => x.SortingType).HasColumnName("SortingType");
            b.Property(x => x.ThresholdA).HasColumnName("ThresholdA");
            b.Property(x => x.ThresholdB).HasColumnName("ThresholdB");
            b.Property(x => x.ThresholdC).HasColumnName("ThresholdC");
            b.Property(x => x.NextStepForGradeA).HasColumnName("NextStepForGradeA");
            b.Property(x => x.NextStepForGradeB).HasColumnName("NextStepForGradeB");
            b.Property(x => x.NextStepForGradeC).HasColumnName("NextStepForGradeC");
            b.Property(x => x.InputProductId).HasColumnName("InputProductId");
            b.Property(x => x.OutputProductId).HasColumnName("OutputProductId");
            b.Property(x => x.YieldRate).HasColumnName("YieldRate");
        });

        builder.Entity<ProcessParameter>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "ProcessParameters", AbpDemoConsts.DbSchema);
            b.Property(x => x.ParameterName).IsRequired().HasMaxLength(100);
            b.Property(x => x.ParameterCode).IsRequired().HasMaxLength(50);
            b.Property(x => x.Unit).HasMaxLength(20);

            // 配置 ParameterRange 为 Owned Entity（值对象）
            b.OwnsOne(p => p.Range, range =>
            {
                range.Property(r => r.MinValue).HasColumnName("RangeMinValue");
                range.Property(r => r.MaxValue).HasColumnName("RangeMaxValue");
                range.Property(r => r.DefaultValue).HasColumnName("RangeDefaultValue");
                range.Property(r => r.Tolerance).HasColumnName("RangeTolerance");
            });

            // 光伏特有字段（SPC）
            b.Property(x => x.UCL).HasColumnName("UCL");
            b.Property(x => x.LCL).HasColumnName("LCL");
            b.Property(x => x.USL).HasColumnName("USL");
            b.Property(x => x.LSL).HasColumnName("LSL");
            b.Property(x => x.SamplingIntervalSeconds).HasColumnName("SamplingIntervalSeconds");
        });

        builder.Entity<ProcessDocument>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "ProcessDocuments", AbpDemoConsts.DbSchema);
            b.Property(x => x.DocumentName).IsRequired().HasMaxLength(200);
            b.Property(x => x.FilePath).HasMaxLength(500);
            b.Property(x => x.Version).HasMaxLength(20);
        });

        builder.Entity<EngineeringChange>(b =>
        {
            b.ToTable(AbpDemoConsts.DbTablePrefix + "EngineeringChanges", AbpDemoConsts.DbSchema);
            b.Property(x => x.EcnNo).IsRequired().HasMaxLength(50);
            b.Property(x => x.Title).IsRequired().HasMaxLength(200);
            b.HasIndex(x => x.EcnNo).IsUnique();
        });
    }
}