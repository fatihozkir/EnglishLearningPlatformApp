using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace EnglishLearningPlatformApp.Reading.EntityFrameworkCore;

[ConnectionStringName(ReadingDbProperties.ConnectionStringName)]
public class ReadingDbContext : AbpDbContext<ReadingDbContext>, IReadingDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * public DbSet<Question> Questions { get; set; }
     */

    public ReadingDbContext(DbContextOptions<ReadingDbContext> options)
        : base(options)
    {

    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ConfigureReading();
    }
}
