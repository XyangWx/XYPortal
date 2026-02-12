using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.LinkBoard.Links;

public interface ILinkReviewAppService : IApplicationService
{
    Task<PagedResultDto<LinkDto>> GetListAsync(GetLinkListInput input);
    Task ReviewAsync(Guid id, ReviewInput input);
}
