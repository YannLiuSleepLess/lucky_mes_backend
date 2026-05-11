using Volo.Abp.Modularity;

namespace AbpDemo;

[DependsOn(
    typeof(AbpDemoDomainModule),
    typeof(AbpDemoTestBaseModule)
)]
public class AbpDemoDomainTestModule : AbpModule
{

}
