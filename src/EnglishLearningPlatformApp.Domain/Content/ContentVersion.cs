using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using System.Collections.Generic;
using System.Linq;

namespace EnglishLearningPlatformApp.Content;

public class ContentVersion : Entity<Guid>, IMultiTenant
{
    private readonly List<ContentSection> _sections = [];
    public Guid? TenantId { get; private set; }
    public Guid ContentItemId { get; private set; }
    public int VersionNumber { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public ContentLifecycleStatus Lifecycle { get; private set; }
    public DateTime? PublishedAt { get; private set; }
    public IReadOnlyCollection<ContentSection> Sections => _sections.AsReadOnly();

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

    internal ContentSection AddSection(Guid sectionId, string heading, string body)
    {
        EnsureDraft();
        var section = new ContentSection(sectionId, Id, TenantId, _sections.Count + 1, heading, body);
        _sections.Add(section);
        return section;
    }

    internal void UpdateSection(Guid sectionId, string heading, string body)
    {
        EnsureDraft();
        GetSection(sectionId).Update(heading, body);
    }

    internal void RemoveSection(Guid sectionId)
    {
        EnsureDraft();
        _sections.Remove(GetSection(sectionId));
        NormalizePositions();
    }

    internal void ReorderSections(IReadOnlyList<Guid> orderedSectionIds)
    {
        EnsureDraft();
        if (orderedSectionIds.Count != _sections.Count ||
            orderedSectionIds.Distinct().Count() != _sections.Count ||
            orderedSectionIds.Any(id => _sections.All(section => section.Id != id)))
        {
            throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentSectionOrderInvalid);
        }

        for (var index = 0; index < orderedSectionIds.Count; index++)
        {
            GetSection(orderedSectionIds[index]).MoveTo(index + 1);
        }
    }

    internal ContentVersion CopyAsDraft(Guid versionId, Func<Guid> sectionIdGenerator)
    {
        var revision = new ContentVersion(versionId, ContentItemId, TenantId, VersionNumber + 1, Title);
        foreach (var section in _sections.OrderBy(x => x.Position))
        {
            revision.AddSection(sectionIdGenerator(), section.Heading, section.Body);
        }

        return revision;
    }

    private ContentSection GetSection(Guid sectionId) =>
        _sections.SingleOrDefault(x => x.Id == sectionId)
        ?? throw new BusinessException(EnglishLearningPlatformAppDomainErrorCodes.ContentSectionNotFound);

    private void NormalizePositions()
    {
        var ordered = _sections.OrderBy(x => x.Position).ToList();
        for (var index = 0; index < ordered.Count; index++)
        {
            ordered[index].MoveTo(index + 1);
        }
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
