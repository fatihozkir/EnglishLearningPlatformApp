using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class ReadingInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<ReadingInstallerModule>();
        });
    }
}
