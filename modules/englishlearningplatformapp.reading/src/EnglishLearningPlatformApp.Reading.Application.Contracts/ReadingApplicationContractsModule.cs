using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.Authorization;

namespace EnglishLearningPlatformApp.Reading;

[DependsOn(
    typeof(ReadingDomainSharedModule),
    typeof(AbpDddApplicationContractsModule),
    typeof(AbpAuthorizationModule)
    )]
public class ReadingApplicationContractsModule : AbpModule
{

}
