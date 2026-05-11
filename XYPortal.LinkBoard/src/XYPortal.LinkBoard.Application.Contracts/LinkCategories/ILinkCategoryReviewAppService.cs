using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.LinkBoard.LinkCategories;

public interface ILinkCategoryReviewAppService : IApplicationService
{
    Task<PagedResultDto<LinkCategoryDto>> GetListAsync(GetLinkCategoryListInput input);
    Task ReviewAsync(Guid id, ReviewInput input);
}
