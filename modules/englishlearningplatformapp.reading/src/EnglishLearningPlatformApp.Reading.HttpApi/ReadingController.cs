using EnglishLearningPlatformApp.Reading.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EnglishLearningPlatformApp.Reading;

public abstract class ReadingController : AbpControllerBase
{
    protected ReadingController()
    {
        LocalizationResource = typeof(ReadingResource);
    }
}
