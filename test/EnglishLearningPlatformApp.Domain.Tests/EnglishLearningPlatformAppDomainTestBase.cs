using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp;

/* Inherit from this class for your domain layer tests. */
public abstract class EnglishLearningPlatformAppDomainTestBase<TStartupModule> : EnglishLearningPlatformAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
