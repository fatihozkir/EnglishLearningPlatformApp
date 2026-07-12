using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(ReadingApplicationModule),
    typeof(ReadingDomainTestModule)
    )]
public class ReadingApplicationTestModule : AbpModule
{

}
