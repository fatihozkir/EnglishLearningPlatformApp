using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.EntityFrameworkCore;
using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading.EntityFrameworkCore;

[DependsOn(
    typeof(ReadingDomainModule),
    typeof(AbpEntityFrameworkCoreModule)
)]
public class ReadingEntityFrameworkCoreModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddAbpDbContext<ReadingDbContext>(options =>
        {
            options.AddDefaultRepositories<IReadingDbContext>(includeAllEntities: true);
            
            /* Add custom repositories here. Example:
            * options.AddRepository<Question, EfCoreQuestionRepository>();
            */
        });
    }
}
