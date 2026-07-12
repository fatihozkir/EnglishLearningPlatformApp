using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace EnglishLearningPlatformApp.Content;

public interface IContentAdminAppService : IApplicationService
{
    Task<ContentItemDto> GetAsync(Guid id);
    Task<PublishedContentVersionDto> GetPublishedAsync(Guid id);
    Task<ContentItemDto> CreateAsync(CreateContentInput input);
    Task<ContentItemDto> UpdateDraftAsync(Guid id, UpdateContentDraftInput input);
    Task<ContentItemDto> PublishAsync(Guid id, ContentConcurrencyInput input);
    Task<ContentItemDto> CreateRevisionAsync(Guid id, ContentConcurrencyInput input);
    Task<ContentItemDto> ArchiveAsync(Guid id, ContentConcurrencyInput input);
}
