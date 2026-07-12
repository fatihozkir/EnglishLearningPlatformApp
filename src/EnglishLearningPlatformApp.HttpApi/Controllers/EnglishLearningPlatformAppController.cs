using EnglishLearningPlatformApp.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace EnglishLearningPlatformApp.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class EnglishLearningPlatformAppController : AbpControllerBase
{
    protected EnglishLearningPlatformAppController()
    {
        LocalizationResource = typeof(EnglishLearningPlatformAppResource);
    }
}
