using System.Threading.Tasks;

namespace AbpDemo.Data;

public interface IAbpDemoDbSchemaMigrator
{
    Task MigrateAsync();
}