using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace AbpDemo.Data;

/* This is used if database provider does't define
 * IAbpDemoDbSchemaMigrator implementation.
 */
public class NullAbpDemoDbSchemaMigrator : IAbpDemoDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}