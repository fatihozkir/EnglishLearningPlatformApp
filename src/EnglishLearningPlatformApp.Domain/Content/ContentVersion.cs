using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Content;

public class ContentVersion : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ContentItemId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ContentLifecycleStatus Lifecycle { get; private set; }
    public DateTime? PublishedAt { get; private set; }

    protected ContentVersion() { }

    internal ContentVersion(Guid id, Guid contentItemId, Guid? tenantId, int versionNumber, string title)
        : base(id)
    {
        ContentItemId = contentItemId;
        TenantId = tenantId;
        VersionNumber = versionNumber;
        Lifecycle = ContentLifecycleStatus.Draft;
        SetTitle(title);
    }

    internal void UpdateDraft(string title)
    {
        EnsureDraft();
        SetTitle(title);
    }

    internal void Publish(DateTime publishedAt)
    {
        EnsureDraft();
        Lifecycle = ContentLifecycleStatus.Published;
        PublishedAt = publishedAt;
    }

    private void EnsureDraft()
    {
        if (Lifecycle != ContentLifecycleStatus.Draft)
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentVersionImmutable);
        }
    }

    private void SetTitle(string title)
    {
        Title = Check.NotNullOrWhiteSpace(title, nameof(title), ContentConsts.MaxTitleLength);
    }
}
