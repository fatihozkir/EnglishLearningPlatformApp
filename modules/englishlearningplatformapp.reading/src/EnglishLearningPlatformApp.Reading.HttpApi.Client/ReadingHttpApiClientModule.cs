using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(ReadingApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class ReadingHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(ReadingApplicationContractsModule).Assembly,
            ReadingRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ReadingHttpApiClientModule>();
        });

    }
}
