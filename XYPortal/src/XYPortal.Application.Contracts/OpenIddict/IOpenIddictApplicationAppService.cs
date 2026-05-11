using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace XYPortal.OpenIddict;

public interface IOpenIddictApplicationAppService : IApplicationService
{
    Task<OpenIddictApplicationDto> GetAsync(Guid id);
    Task<PagedResultDto<OpenIddictApplicationDto>> GetListAsync(GetOpenIddictApplicationListInput input);
    Task<OpenIddictApplicationDto> CreateAsync(CreateOpenIddictApplicationDto input);
    Task<OpenIddictApplicationDto> UpdateAsync(Guid id, UpdateOpenIddictApplicationDto input);
    Task DeleteAsync(Guid id);
}
