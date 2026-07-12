using EnglishLearningPlatformApp.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(EnglishLearningPlatformAppEntityFrameworkCoreModule),
    typeof(EnglishLearningPlatformAppApplicationContractsModule)
)]
public class EnglishLearningPlatformAppDbMigratorModule : AbpModule
{
}
