using System;
using System.Collections.Generic;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Content;

public class ContentItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    private readonly List<ContentVersion> _versions = [];

    public Guid? TenantId { get; private set; }
    public ContentType Type { get; private set; }
    public ContentItemStatus Status { get; private set; }
    public int ChangeSequence { get; private set; }
    public IReadOnlyCollection<ContentVersion> Versions => _versions.AsReadOnly();

    protected ContentItem() { }

    public ContentItem(Guid id, Guid? tenantId, ContentType type, Guid firstVersionId, string title)
        : base(id)
    {
        TenantId = tenantId;
        Type = type;
        Status = ContentItemStatus.Draft;
        _versions.Add(new ContentVersion(firstVersionId, id, tenantId, 1, title));
    }

    public void UpdateDraft(string title)
    {
        EnsureNotArchived();
        GetDraft().UpdateDraft(title);
        Touch();
    }

    public void PublishDraft(DateTime publishedAt)
    {
        EnsureNotArchived();
        GetDraft().Publish(publishedAt);
        Status = ContentItemStatus.Published;
        Touch();
    }

    public ContentVersion CreateRevision(Guid versionId)
    {
        EnsureNotArchived();
        if (_versions.Any(x => x.Lifecycle == ContentLifecycleStatus.Draft))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftAlreadyExists);
        }

        var latest = _versions.OrderByDescending(x => x.VersionNumber).First();
        var revision = new ContentVersion(versionId, Id, TenantId, latest.VersionNumber + 1, latest.Title);
        _versions.Add(revision);
        Touch();
        return revision;
    }

    public void Archive()
    {
        EnsureNotArchived();
        Status = ContentItemStatus.Archived;
        Touch();
    }

    private ContentVersion GetDraft()
    {
        return _versions.SingleOrDefault(x => x.Lifecycle == ContentLifecycleStatus.Draft)
            ?? throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftMissing);
    }

    private void EnsureNotArchived()
    {
        if (Status == ContentItemStatus.Archived)
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentArchived);
        }
    }

    private void Touch()
    {
        ChangeSequence++;
    }
}
