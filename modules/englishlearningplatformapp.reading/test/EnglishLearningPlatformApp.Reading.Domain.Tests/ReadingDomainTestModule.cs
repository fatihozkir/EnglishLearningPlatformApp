using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(ReadingDomainModule),
    typeof(ReadingTestBaseModule)
)]
public class ReadingDomainTestModule : AbpModule
{

}
