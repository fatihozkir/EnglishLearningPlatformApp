using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp;

[DependsOn(
    typeof(EnglishLearningPlatformAppApplicationModule),
    typeof(EnglishLearningPlatformAppDomainTestModule)
)]
public class EnglishLearningPlatformAppApplicationTestModule : AbpModule
{

}
