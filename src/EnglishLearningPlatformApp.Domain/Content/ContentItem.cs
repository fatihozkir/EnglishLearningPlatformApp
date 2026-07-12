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

    public ContentSection AddSection(Guid sectionId, string heading, string body)
    {
        EnsureNotArchived();
        var section = GetDraft().AddSection(sectionId, heading, body);
        Touch();
        return section;
    }

    public void UpdateSection(Guid sectionId, string heading, string body)
    {
        EnsureNotArchived();
        GetDraft().UpdateSection(sectionId, heading, body);
        Touch();
    }

    public void RemoveSection(Guid sectionId)
    {
        EnsureNotArchived();
        GetDraft().RemoveSection(sectionId);
        Touch();
    }

    public void ReorderSections(IReadOnlyList<Guid> orderedSectionIds)
    {
        EnsureNotArchived();
        GetDraft().ReorderSections(orderedSectionIds);
        Touch();
    }

    public ContentVersion CreateRevision(Guid versionId, Func<Guid> sectionIdGenerator)
    {
        EnsureNotArchived();
        if (_versions.Any(x => x.Lifecycle == ContentLifecycleStatus.Draft))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentDraftAlreadyExists);
        }

        var latest = _versions.OrderByDescending(x => x.VersionNumber).First();
        var revision = latest.CopyAsDraft(versionId, sectionIdGenerator);
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
