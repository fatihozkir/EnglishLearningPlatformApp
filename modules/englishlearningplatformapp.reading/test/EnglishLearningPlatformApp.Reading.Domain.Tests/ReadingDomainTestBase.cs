using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class ReadingDomainTestBase<TStartupModule> : ReadingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
