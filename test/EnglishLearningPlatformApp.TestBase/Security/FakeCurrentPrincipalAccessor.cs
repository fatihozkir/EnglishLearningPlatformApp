using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Security.Claims;

namespace EnglishLearningPlatformApp.Security;

[Dependency(ReplaceServices = true)]
public class FakeCurrentPrincipalAccessor : ThreadCurrentPrincipalAccessor
{
    private readonly AsyncLocal<ClaimsPrincipal?> _overridePrincipal = new();

    protected override ClaimsPrincipal GetClaimsPrincipal()
    {
        return _overridePrincipal.Value ?? GetPrincipal();
    }

    public IDisposable ChangeTo(ClaimsPrincipal principal)
    {
        var previous = _overridePrincipal.Value;
        _overridePrincipal.Value = principal;
        return new RestorePrincipal(() => _overridePrincipal.Value = previous);
    }

    private ClaimsPrincipal GetPrincipal()
    {
        return new ClaimsPrincipal(new ClaimsIdentity(new List<Claim>
        {
            new Claim(AbpClaimTypes.UserId, "2e701e62-0953-4dd3-910b-dc6cc93ccb0d"),
            new Claim(AbpClaimTypes.UserName, "admin"),
            new Claim(AbpClaimTypes.Email, "admin@abp.io")
        }, "Test"));
    }

    private sealed class RestorePrincipal(Action restore) : IDisposable
    {
        public void Dispose() => restore();
    }
}
