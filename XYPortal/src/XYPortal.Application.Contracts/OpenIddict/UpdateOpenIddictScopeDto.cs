using System.Collections.Generic;

namespace XYPortal.OpenIddict;

public class UpdateOpenIddictScopeDto
{
    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public List<string> Resources { get; set; } = [];
}
