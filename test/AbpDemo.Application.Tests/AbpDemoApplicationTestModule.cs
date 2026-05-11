using Volo.Abp.Modularity;

namespace AbpDemo;

[DependsOn(
    typeof(AbpDemoApplicationModule),
    typeof(AbpDemoDomainTestModule)
)]
public class AbpDemoApplicationTestModule : AbpModule
{

}
