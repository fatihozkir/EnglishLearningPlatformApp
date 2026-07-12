using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishLearningPlatformApp.Reading.EntityFrameworkCore;

[ConnectionStringName(ReadingDbProperties.ConnectionStringName)]
public interface IReadingDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
