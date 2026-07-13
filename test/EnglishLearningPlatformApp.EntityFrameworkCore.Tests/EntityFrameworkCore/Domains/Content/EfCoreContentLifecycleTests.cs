using System;
using System.Linq;
using System.Threading.Tasks;
using EnglishLearningPlatformApp.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Xunit;

namespace EnglishLearningPlatformApp.Content;

[Collection(EnglishLearningPlatformAppTestConsts.CollectionDefinitionName)]
public class EfCoreContentLifecycleTests : EnglishLearningPlatformAppEntityFrameworkCoreTestBase
{
    private readonly IRepository<ContentItem, Guid> _repository;
    private readonly IRepository<ContentVersion, Guid> _versionRepository;
    private readonly IRepository<ContentSection, Guid> _sectionRepository;
    private readonly IRepository<ContentQuestion, Guid> _questionRepository;
    private readonly IRepository<ContentQuestionOption, Guid> _optionRepository;
    private readonly ICurrentTenant _currentTenant;

    public EfCoreContentLifecycleTests()
    {
        _repository = GetRequiredService<IRepository<ContentItem, Guid>>();
        _versionRepository = GetRequiredService<IRepository<ContentVersion, Guid>>();
        _sectionRepository = GetRequiredService<IRepository<ContentSection, Guid>>();
        _questionRepository = GetRequiredService<IRepository<ContentQuestion, Guid>>();
        _optionRepository = GetRequiredService<IRepository<ContentQuestionOption, Guid>>();
        _currentTenant = GetRequiredService<ICurrentTenant>();
    }

    [Fact]
    public async Task Aggregate_Should_Persist_Versions_And_Preserve_Published_Snapshot()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(async () =>
            {
                var item = new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Version one");
                var section = item.AddSection(Guid.NewGuid(), "Section one", "Original passage");
                item.AddQuestion(section.Id, Guid.NewGuid(), QuestionType.SingleChoice, "Choose", "{\"correct\":1}",
                    [new("A"), new("B")], Guid.NewGuid);
                item.PublishDraft(new DateTime(2026, 7, 13, 0, 0, 0, DateTimeKind.Utc));
                item.CreateRevision(Guid.NewGuid(), Guid.NewGuid);
                item.UpdateDraft("Version two");
                await _repository.InsertAsync(item);
            });

            var reloaded = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));
            reloaded.Versions.Count.ShouldBe(2);
            reloaded.Versions.OrderBy(x => x.VersionNumber).Select(x => x.Title)
                .ShouldBe(new[] { "Version one", "Version two" });
            reloaded.Versions.OrderBy(x => x.VersionNumber).Select(x => x.Sections.Single().Body)
                .ShouldBe(new[] { "Original passage", "Original passage" });
            reloaded.Versions.Select(x => x.Sections.Single().Questions.Single().Options.Count).ShouldAllBe(x => x == 2);
        }
    }

    [Fact]
    public async Task Tenant_And_Host_Filters_Should_Hide_Other_Tenant_Content()
    {
        var tenantOne = Guid.NewGuid();
        var tenantTwo = Guid.NewGuid();
        var itemId = Guid.NewGuid();

        using (_currentTenant.Change(tenantOne))
        {
            await WithUnitOfWorkAsync(() =>
            {
                var item = new ContentItem(itemId, tenantOne, ContentType.Listening, Guid.NewGuid(), "Tenant one");
                var section = item.AddSection(Guid.NewGuid(), "Private", "Tenant passage");
                item.AddQuestion(section.Id, Guid.NewGuid(), QuestionType.Matching, "Private question", "{\"pairs\":[]}",
                    [new("A", "One"), new("B", "Two")], Guid.NewGuid);
                return _repository.InsertAsync(item);
            });
            (await _repository.FindAsync(itemId)).ShouldNotBeNull();
        }

        using (_currentTenant.Change(tenantTwo))
        {
            (await _repository.FindAsync(itemId)).ShouldBeNull();
            (await _versionRepository.GetCountAsync()).ShouldBe(0);
            (await _sectionRepository.GetCountAsync()).ShouldBe(0);
            (await _questionRepository.GetCountAsync()).ShouldBe(0);
            (await _optionRepository.GetCountAsync()).ShouldBe(0);
        }

        using (_currentTenant.Change(null))
        {
            (await _repository.FindAsync(itemId)).ShouldBeNull();
            (await _versionRepository.GetCountAsync()).ShouldBe(0);
            (await _sectionRepository.GetCountAsync()).ShouldBe(0);
            (await _questionRepository.GetCountAsync()).ShouldBe(0);
            (await _optionRepository.GetCountAsync()).ShouldBe(0);
        }
    }

    [Fact]
    public async Task All_Question_Types_Should_Persist_With_Server_Answers()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() =>
            {
                var item = new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "All types");
                var section = item.AddSection(Guid.NewGuid(), "Questions", "Passage");
                foreach (var type in Enum.GetValues<QuestionType>())
                {
                    item.AddQuestion(section.Id, Guid.NewGuid(), type, type.ToString(), "{\"serverOnly\":true}",
                        type switch
                        {
                            QuestionType.Matching => [new("A", "One"), new("B", "Two")],
                            QuestionType.SingleChoice or QuestionType.MultipleSelect or QuestionType.Ordering =>
                                [new("A"), new("B")],
                            _ => []
                        }, Guid.NewGuid);
                }

                return _repository.InsertAsync(item);
            });

            var reloaded = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));
            var questions = reloaded.Versions.Single().Sections.Single().Questions.OrderBy(x => x.Position).ToList();
            questions.Select(x => x.Type).ShouldBe(Enum.GetValues<QuestionType>());
            questions.ShouldAllBe(x => x.AnswerDefinitionJson == "{\"serverOnly\":true}");
        }
    }

    [Fact]
    public async Task Undefined_Question_Type_Should_Be_Rejected_Before_Persistence()
    {
        var tenantId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() =>
            {
                var item = new ContentItem(Guid.NewGuid(), tenantId, ContentType.Reading, Guid.NewGuid(), "Invalid type");
                var section = item.AddSection(Guid.NewGuid(), "Questions", "Passage");

                Should.Throw<Volo.Abp.BusinessException>(() => item.AddQuestion(
                        section.Id, Guid.NewGuid(), (QuestionType)99, "Prompt", "true", [], Guid.NewGuid))
                    .Code.ShouldBe(EnglishLearningPlatformAppDomainErrorCodes.QuestionTypeInvalid);

                return Task.CompletedTask;
            });
        }

        (await _questionRepository.GetCountAsync()).ShouldBe(0);
    }

    [Fact]
    public async Task Section_Reorder_Should_Persist_Contiguous_Positions()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() =>
            {
                var item = new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Reorder");
                item.AddSection(Guid.NewGuid(), "One", "Body one");
                item.AddSection(Guid.NewGuid(), "Two", "Body two");
                item.AddSection(Guid.NewGuid(), "Three", "Body three");
                return _repository.InsertAsync(item);
            });

            await WithUnitOfWorkAsync(async () =>
            {
                var item = await _repository.GetAsync(itemId, includeDetails: true);
                var reversed = item.Versions.Single().Sections.OrderByDescending(x => x.Position).Select(x => x.Id).ToList();
                item.ReorderSections(reversed);
                await _repository.UpdateAsync(item, autoSave: true);
            });

            var reloaded = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));
            reloaded.Versions.Single().Sections.OrderBy(x => x.Position).Select(x => x.Heading)
                .ShouldBe(new[] { "Three", "Two", "One" });

            await WithUnitOfWorkAsync(async () =>
            {
                var item = await _repository.GetAsync(itemId, includeDetails: true);
                var first = item.Versions.Single().Sections.Single(x => x.Position == 1);
                item.RemoveSection(first.Id);
                await _repository.UpdateAsync(item, autoSave: true);
            });

            var afterRemoval = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));
            afterRemoval.Versions.Single().Sections.OrderBy(x => x.Position).Select(x => x.Position)
                .ShouldBe(new[] { 1, 2 });
        }
    }

    [Fact]
    public async Task Database_Should_Reject_A_Second_Draft_For_The_Same_Item()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _repository.InsertAsync(
                new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Draft one")));

            await Should.ThrowAsync<DbUpdateException>(() => WithUnitOfWorkAsync(() =>
                _versionRepository.InsertAsync(
                    new ContentVersion(Guid.NewGuid(), itemId, tenantId, 2, "Draft two"), autoSave: true)));
        }
    }

    [Fact]
    public async Task Stale_Concurrent_Draft_Edit_Should_Be_Rejected()
    {
        var tenantId = Guid.NewGuid();
        var itemId = Guid.NewGuid();
        using (_currentTenant.Change(tenantId))
        {
            await WithUnitOfWorkAsync(() => _repository.InsertAsync(
                new ContentItem(itemId, tenantId, ContentType.Reading, Guid.NewGuid(), "Original")));
        }

        using (_currentTenant.Change(tenantId))
        {
            var staleEditor = await WithUnitOfWorkAsync(() => _repository.GetAsync(itemId, includeDetails: true));

            await WithUnitOfWorkAsync(async () =>
            {
                var firstEditor = await _repository.GetAsync(itemId, includeDetails: true);
                firstEditor.UpdateDraft("First editor");
                await _repository.UpdateAsync(firstEditor, autoSave: true);
            });

            staleEditor.UpdateDraft("Stale editor");
            await Should.ThrowAsync<AbpDbConcurrencyException>(() => WithUnitOfWorkAsync(() =>
                _repository.UpdateAsync(staleEditor, autoSave: true)));
        }
    }
}
