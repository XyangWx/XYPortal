using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.OpenIddict;

public interface IOpenIddictScopeAppService : IApplicationService
{
    Task<OpenIddictScopeDto> GetAsync(Guid id);
    Task<PagedResultDto<OpenIddictScopeDto>> GetListAsync(GetOpenIddictScopeListInput input);
    Task<OpenIddictScopeDto> CreateAsync(CreateOpenIddictScopeDto input);
    Task<OpenIddictScopeDto> UpdateAsync(Guid id, UpdateOpenIddictScopeDto input);
    Task DeleteAsync(Guid id);
}
