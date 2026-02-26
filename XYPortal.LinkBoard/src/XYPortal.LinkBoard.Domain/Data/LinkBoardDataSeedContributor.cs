using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using XYPortal.LinkBoard.Entities;
using XYPortal.LinkBoard.Repositories;

namespace XYPortal.LinkBoard.Data;

public class LinkBoardDataSeedContributor : IDataSeedContributor, ITransientDependency
{
    private readonly ILinkCategoryRepository _categoryRepository;

    public LinkBoardDataSeedContributor(ILinkCategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    [UnitOfWork]
    public virtual async Task SeedAsync(DataSeedContext context)
    {
        await SeedDefaultCategoryAsync();
    }

    private async Task SeedDefaultCategoryAsync()
    {
        var existing = await _categoryRepository.FindAsync(LinkBoardConsts.DefaultCategory.Id);
        if (existing != null)
        {
            return;
        }

        var defaultCategory = new LinkCategory(
            LinkBoardConsts.DefaultCategory.Id,
            LinkBoardConsts.DefaultCategory.Name,
            isPublic: true)
        {
            DisplayName = LinkBoardConsts.DefaultCategory.DisplayName,
            Description = LinkBoardConsts.DefaultCategory.Description,
            Icon = LinkBoardConsts.DefaultCategory.Icon,
            SortOrder = 0,
            Status = ReviewStatus.Approved,
            IsDefault = true
        };

        await _categoryRepository.InsertAsync(defaultCategory);
    }
}
