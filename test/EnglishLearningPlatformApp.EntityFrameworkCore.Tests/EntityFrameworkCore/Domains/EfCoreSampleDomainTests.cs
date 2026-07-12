using EnglishLearningPlatformApp.Samples;
using Xunit;

namespace EnglishLearningPlatformApp.EntityFrameworkCore.Domains;

[Collection(EnglishLearningPlatformAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<EnglishLearningPlatformAppEntityFrameworkCoreTestModule>
{

}
