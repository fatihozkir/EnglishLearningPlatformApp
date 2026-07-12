using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Authorization;

namespace EnglishLearningPlatformApp.Content;

public class ConfigurableContentPermissionAuthorizer : IContentPermissionAuthorizer
{
    private readonly AsyncLocal<bool> _deny = new();

    public IDisposable DenyAll()
    {
        var previous = _deny.Value;
        _deny.Value = true;
        return new Restore(() => _deny.Value = previous);
    }

    public Task CheckAsync(string permission)
    {
        if (_deny.Value)
        {
            throw new AbpAuthorizationException($"Permission denied: {permission}");
        }

        return Task.CompletedTask;
    }

    private sealed class Restore(Action action) : IDisposable
    {
        public void Dispose() => action();
    }
}
