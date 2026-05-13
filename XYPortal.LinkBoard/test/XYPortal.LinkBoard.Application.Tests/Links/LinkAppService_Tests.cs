using System;
using System.Threading.Tasks;
using Shouldly;
using Volo.Abp.Modularity;
using Volo.Abp.Users;
using Xunit;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Links;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.Links;

public abstract class LinkAppService_Tests<TStartupModule> : LinkBoardApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly ILinkAppService _linkAppService;
    private readonly ILinkRepository _linkRepository;

    protected LinkAppService_Tests()
    {
        _linkAppService = GetRequiredService<ILinkAppService>();
        _linkRepository = GetRequiredService<ILinkRepository>();
    }

    [Fact]
    public async Task QueryMaxIndexAsync_Should_Return_1_When_No_Links_In_Category()
    {
        // Arrange
        var categoryId = Guid.NewGuid();
        var input = new QueryMaxIndexInput { CategoryId = categoryId };

        // Act
        var result = await _linkAppService.QueryMaxIndexAsync(input);

        // Assert
        result.Index.ShouldBe(1);
    }

    [Fact]
    public async Task QueryMaxIndexAsync_Should_Return_Max_SortOrder_Plus_1()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        // Create a category first
        await WithUnitOfWorkAsync(async () =>
        {
            var category = new LinkCategory(categoryId, "Test Category", isPublic: true);
            var categoryRepo = GetRequiredService<ILinkCategoryRepository>();
            await categoryRepo.InsertAsync(category);
        });

        // Create links with SortOrder 1, 2, 3
        await WithUnitOfWorkAsync(async () =>
        {
            await _linkRepository.InsertAsync(new Link(Guid.NewGuid(), categoryId, "Link 1", "https://example.com/1") { SortOrder = 1 });
            await _linkRepository.InsertAsync(new Link(Guid.NewGuid(), categoryId, "Link 2", "https://example.com/2") { SortOrder = 2 });
            await _linkRepository.InsertAsync(new Link(Guid.NewGuid(), categoryId, "Link 3", "https://example.com/3") { SortOrder = 3 });
        });

        var input = new QueryMaxIndexInput { CategoryId = categoryId };

        // Act
        var result = await _linkAppService.QueryMaxIndexAsync(input);

        // Assert
        result.Index.ShouldBe(4); // Max SortOrder (3) + 1
    }

    [Fact]
    public async Task QueryMaxIndexAsync_Should_Not_Count_Deleted_Links()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        await WithUnitOfWorkAsync(async () =>
        {
            var category = new LinkCategory(categoryId, "Test Category", isPublic: true);
            var categoryRepo = GetRequiredService<ILinkCategoryRepository>();
            await categoryRepo.InsertAsync(category);
        });

        await WithUnitOfWorkAsync(async () =>
        {
            var link1 = new Link(Guid.NewGuid(), categoryId, "Link 1", "https://example.com/1") { SortOrder = 1 };
            var link2 = new Link(Guid.NewGuid(), categoryId, "Link 2", "https://example.com/2") { SortOrder = 2 };
            await _linkRepository.InsertAsync(link1);
            await _linkRepository.InsertAsync(link2);
        });

        // Soft delete one link
        await WithUnitOfWorkAsync(async () =>
        {
            var link = await _linkRepository.GetListAsync(categoryId, null, null, null, null, false, nameof(Link.SortOrder), 0, 10);
            if (link.Count > 0)
            {
                await _linkRepository.DeleteAsync(link[0]);
            }
        });

        var input = new QueryMaxIndexInput { CategoryId = categoryId };

        // Act
        var result = await _linkAppService.QueryMaxIndexAsync(input);

        // Assert
        result.Index.ShouldBe(3); // Remaining max SortOrder (2) + 1
    }

    [Fact]
    public async Task QueryMaxIndexAsync_Should_Return_1_When_CategoryId_Is_Null()
    {
        // Arrange
        var input = new QueryMaxIndexInput { CategoryId = null };

        // Act
        var result = await _linkAppService.QueryMaxIndexAsync(input);

        // Assert
        result.Index.ShouldBe(1);
    }
}

public class LinkAppService_Tests : LinkAppService_Tests<LinkBoardApplicationTestModule>
{
}
