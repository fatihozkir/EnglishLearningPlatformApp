using EnglishLearningPlatformApp.Reading.Localization;
using Volo.Abp.Application.Services;

namespace EnglishLearningPlatformApp.Reading;

public abstract class ReadingAppService : ApplicationService
{
    protected ReadingAppService()
    {
        LocalizationResource = typeof(ReadingResource);
        ObjectMapperContext = typeof(ReadingApplicationModule);
    }
}
