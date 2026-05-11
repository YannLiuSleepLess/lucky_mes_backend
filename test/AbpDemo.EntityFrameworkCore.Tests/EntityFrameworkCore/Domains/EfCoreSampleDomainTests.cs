using AbpDemo.Samples;
using Xunit;

namespace AbpDemo.EntityFrameworkCore.Domains;

[Collection(AbpDemoTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<AbpDemoEntityFrameworkCoreTestModule>
{

}
