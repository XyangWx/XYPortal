using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.LinkBoard.Links;

public interface ILinkAppService : IApplicationService
{
    Task<LinkDto> GetAsync(Guid id);
    Task<PagedResultDto<LinkDto>> GetListAsync(GetLinkListInput input);
    Task<LinkDto> CreateAsync(CreateLinkDto input);
    Task<LinkDto> UpdateAsync(Guid id, UpdateLinkDto input);
    Task DeleteAsync(Guid id);
    Task SubmitAsync(Guid id);
    Task WithdrawAsync(Guid id);
    Task<PagedResultDto<LinkDto>> GetPublicBoardAsync(GetPublicBoardInput input);
    Task<int> GetMaxLinksAsync();
    Task<QueryMaxIndexOutput> QueryMaxIndexAsync(QueryMaxIndexInput input);
}
