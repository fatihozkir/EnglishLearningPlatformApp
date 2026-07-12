using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(ReadingDomainSharedModule)
)]
public class ReadingDomainModule : AbpModule
{

}
