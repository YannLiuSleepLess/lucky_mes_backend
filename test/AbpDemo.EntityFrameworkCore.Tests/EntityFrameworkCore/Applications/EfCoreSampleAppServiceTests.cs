using AbpDemo.Samples;
using Xunit;

namespace AbpDemo.EntityFrameworkCore.Applications;

[Collection(AbpDemoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<AbpDemoEntityFrameworkCoreTestModule>
{

}
