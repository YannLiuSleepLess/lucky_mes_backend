using Volo.Abp.Modularity;

namespace AbpDemo;

public abstract class AbpDemoApplicationTestBase<TStartupModule> : AbpDemoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
