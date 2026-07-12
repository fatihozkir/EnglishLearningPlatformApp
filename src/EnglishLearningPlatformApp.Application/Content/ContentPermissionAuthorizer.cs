using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization;
using Volo.Abp.DependencyInjection;

namespace EnglishLearningPlatformApp.Content;

public interface IContentPermissionAuthorizer
{
    Task CheckAsync(string permission);
}

[Dependency(ServiceLifetime.Transient)]
public class ContentPermissionAuthorizer(IAuthorizationService authorizationService) : IContentPermissionAuthorizer
{
    public Task CheckAsync(string permission) => authorizationService.CheckAsync(permission);
}
