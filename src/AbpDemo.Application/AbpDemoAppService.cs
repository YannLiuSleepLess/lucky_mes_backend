using AbpDemo.Localization;
using Volo.Abp.Application.Services;

namespace AbpDemo;

/* Inherit your application services from this class.
 */
public abstract class AbpDemoAppService : ApplicationService
{
    protected AbpDemoAppService()
    {
        LocalizationResource = typeof(AbpDemoResource);
    }
}