using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using EnglishLearningPlatformApp.Data;
using Volo.Abp.DependencyInjection;

namespace EnglishLearningPlatformApp.EntityFrameworkCore;

public class EntityFrameworkCoreEnglishLearningPlatformAppDbSchemaMigrator
    : IEnglishLearningPlatformAppDbSchemaMigrator, ITransientDependency
{
    private readonly IServiceProvider _serviceProvider;

    public EntityFrameworkCoreEnglishLearningPlatformAppDbSchemaMigrator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task MigrateAsync()
    {
        /* We intentionally resolving the EnglishLearningPlatformAppDbContext
         * from IServiceProvider (instead of directly injecting it)
         * to properly get the connection string of the current tenant in the
         * current scope.
         */

        await _serviceProvider
            .GetRequiredService<EnglishLearningPlatformAppDbContext>()
            .Database
            .MigrateAsync();
    }
}
