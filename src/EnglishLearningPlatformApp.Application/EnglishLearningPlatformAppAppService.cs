using EnglishLearningPlatformApp.Localization;
using Volo.Abp.Application.Services;

namespace EnglishLearningPlatformApp;

/* Inherit your application services from this class.
 */
public abstract class EnglishLearningPlatformAppAppService : ApplicationService
{
    protected EnglishLearningPlatformAppAppService()
    {
        LocalizationResource = typeof(EnglishLearningPlatformAppResource);
    }
}
