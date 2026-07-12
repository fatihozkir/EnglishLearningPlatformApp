using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EnglishLearningPlatformApp.EntityFrameworkCore;

/* This class is needed for EF Core console commands
 * (like Add-Migration and Update-Database commands) */
public class EnglishLearningPlatformAppDbContextFactory : IDesignTimeDbContextFactory<EnglishLearningPlatformAppDbContext>
{
    public EnglishLearningPlatformAppDbContext CreateDbContext(string[] args)
    {
        var configuration = BuildConfiguration();
        
        EnglishLearningPlatformAppEfCoreEntityExtensionMappings.Configure();

        var builder = new DbContextOptionsBuilder<EnglishLearningPlatformAppDbContext>()
            .UseSqlServer(configuration.GetConnectionString("Default"));
        
        return new EnglishLearningPlatformAppDbContext(builder.Options);
    }

    private static IConfigurationRoot BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../EnglishLearningPlatformApp.DbMigrator/"))
            .AddJsonFile("appsettings.json", optional: false)
            .AddEnvironmentVariables();

        return builder.Build();
    }
}
