using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;

namespace EnglishLearningPlatformApp.Content;

public class ContentSection : Entity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; private set; }
    public Guid ContentVersionId { get; private set; }
    public int Position { get; private set; }
    public string Heading { get; private set; } = string.Empty;
    public string Body { get; private set; } = string.Empty;

    protected ContentSection() { }

    internal ContentSection(
        Guid id,
        Guid contentVersionId,
        Guid? tenantId,
        int position,
        string heading,
        string body) : base(id)
    {
        ContentVersionId = contentVersionId;
        TenantId = tenantId;
        Position = position;
        SetContent(heading, body);
    }

    internal void Update(string heading, string body) => SetContent(heading, body);

    internal void MoveTo(int position) => Position = position;

    private void SetContent(string heading, string body)
    {
        Heading = Check.Length(heading ?? string.Empty, nameof(heading), ContentConsts.MaxSectionHeadingLength)!;
        Body = Check.NotNullOrWhiteSpace(body, nameof(body), ContentConsts.MaxSectionBodyLength);
    }
}
