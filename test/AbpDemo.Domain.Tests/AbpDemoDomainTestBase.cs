using Volo.Abp.Modularity;

namespace AbpDemo;

/* Inherit from this class for your domain layer tests. */
public abstract class AbpDemoDomainTestBase<TStartupModule> : AbpDemoTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
