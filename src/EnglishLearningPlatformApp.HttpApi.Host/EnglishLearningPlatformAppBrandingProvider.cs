using Microsoft.Extensions.Localization;
using EnglishLearningPlatformApp.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Ui.Branding;

namespace EnglishLearningPlatformApp;

[Dependency(ReplaceServices = true)]
public class EnglishLearningPlatformAppBrandingProvider : DefaultBrandingProvider
{
    private IStringLocalizer<EnglishLearningPlatformAppResource> _localizer;

    public EnglishLearningPlatformAppBrandingProvider(IStringLocalizer<EnglishLearningPlatformAppResource> localizer)
    {
        _localizer = localizer;
    }

    public override string AppName => _localizer["AppName"];
}
