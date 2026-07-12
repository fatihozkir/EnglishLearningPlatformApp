using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp;

[DependsOn(
    typeof(EnglishLearningPlatformAppDomainModule),
    typeof(EnglishLearningPlatformAppTestBaseModule)
)]
public class EnglishLearningPlatformAppDomainTestModule : AbpModule
{

}
