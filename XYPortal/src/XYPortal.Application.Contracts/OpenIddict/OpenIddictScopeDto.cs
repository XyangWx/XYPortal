using System;
using System.Collections.Generic;

namespace XYPortal.OpenIddict;

public class OpenIddictScopeDto
{
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = default!;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }
    public List<string> Resources { get; set; } = [];
}
