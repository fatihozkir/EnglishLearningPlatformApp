using Volo.Abp.Modularity;

namespace EnglishLearningPlatformApp.Reading;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class ReadingApplicationTestBase<TStartupModule> : ReadingTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
