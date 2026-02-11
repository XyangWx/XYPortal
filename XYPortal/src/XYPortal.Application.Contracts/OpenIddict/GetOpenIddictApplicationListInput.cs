using Volo.Abp.Application.Dtos;

namespace XYPortal.OpenIddict;

public class GetOpenIddictApplicationListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}
