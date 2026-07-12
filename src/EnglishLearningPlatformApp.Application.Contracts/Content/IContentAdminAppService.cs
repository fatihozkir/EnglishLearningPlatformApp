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
    Task<ContentItemDto> AddSectionAsync(Guid id, AddContentSectionInput input);
    Task<ContentItemDto> UpdateSectionAsync(Guid id, Guid sectionId, UpdateContentSectionInput input);
    Task<ContentItemDto> RemoveSectionAsync(Guid id, Guid sectionId, ContentConcurrencyInput input);
    Task<ContentItemDto> ReorderSectionsAsync(Guid id, ReorderContentSectionsInput input);
    Task<ContentItemDto> AddQuestionAsync(Guid id, AddContentQuestionInput input);
}
