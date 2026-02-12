using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.LinkBoard.LinkCategories;

public interface ILinkCategoryAppService : IApplicationService
{
    Task<LinkCategoryDto> GetAsync(Guid id);
    Task<PagedResultDto<LinkCategoryDto>> GetListAsync(GetLinkCategoryListInput input);
    Task<LinkCategoryDto> CreateAsync(CreateLinkCategoryDto input);
    Task<LinkCategoryDto> UpdateAsync(Guid id, UpdateLinkCategoryDto input);
    Task DeleteAsync(Guid id);
    Task SubmitAsync(Guid id);
    Task WithdrawAsync(Guid id);
}
