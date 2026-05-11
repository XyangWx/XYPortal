using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XYPortal.OpenIddict;

public class CreateOpenIddictScopeDto
{
    [Required]
    public string Name { get; set; } = default!;

    public string? DisplayName { get; set; }

    public string? Description { get; set; }

    public List<string> Resources { get; set; } = [];
}
