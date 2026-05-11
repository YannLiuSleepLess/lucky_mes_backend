using AbpDemo.Localization;
using Microsoft.Extensions.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace AbpDemo;

[Dependency(ReplaceServices = true)]
public class AbpDemoBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<AbpDemoResource> _localizer;

    public AbpDemoBrandingProvider(IStringLocalizer<AbpDemoResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}