using Xunit;

namespace AbpDemo.EntityFrameworkCore;

[CollectionDefinition(AbpDemoTestConsts.CollectionDefinitionName)]
public class AbpDemoEntityFrameworkCoreCollection : ICollectionFixture<AbpDemoEntityFrameworkCoreFixture>
{

}
