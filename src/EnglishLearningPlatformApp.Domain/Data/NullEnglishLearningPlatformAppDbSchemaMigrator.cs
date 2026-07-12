using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;

namespace EnglishLearningPlatformApp.Data;

/* This is used if database provider does't define
 * IEnglishLearningPlatformAppDbSchemaMigrator implementation.
 */
public class NullEnglishLearningPlatformAppDbSchemaMigrator : IEnglishLearningPlatformAppDbSchemaMigrator, ITransientDependency
{
    public Task MigrateAsync()
    {
        return Task.CompletedTask;
    }
}
