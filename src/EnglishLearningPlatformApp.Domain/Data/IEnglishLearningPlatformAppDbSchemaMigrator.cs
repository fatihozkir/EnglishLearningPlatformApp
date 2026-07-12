using System.Threading.Tasks;

namespace EnglishLearningPlatformApp.Data;

public interface IEnglishLearningPlatformAppDbSchemaMigrator
{
    Task MigrateAsync();
}
