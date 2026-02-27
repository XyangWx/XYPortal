using Volo.Abp.Application.Dtos;

namespace XYPortal.OpenIddict;

public class GetOpenIddictScopeListInput : PagedAndSortedResultRequestDto
{
    public string? Filter
    {
        get => field;
        set => field = value;
    }
}
