using Localization.Resources.AbpUi;
using EnglishLearningPlatformApp.Reading.Localization;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Localization;
using Volo.Abp.Modularity;
using Microsoft.Extensions.DependencyInjection;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(ReadingApplicationContractsModule),
    typeof(AbpAspNetCoreMvcModule))]
public class ReadingHttpApiModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        PreConfigure<IMvcBuilder>(mvcBuilder =>
        {
            mvcBuilder.AddApplicationPartIfNotExists(typeof(ReadingHttpApiModule).Assembly);
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpLocalizationOptions>(options =>
        {
            options.Resources
                .Get<ReadingResource>()
                .AddBaseTypes(typeof(AbpUiResource));
        });
    }
}
