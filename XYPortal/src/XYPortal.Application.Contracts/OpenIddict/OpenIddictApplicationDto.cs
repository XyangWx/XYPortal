using System;
using System.Collections.Generic;

namespace XYPortal.OpenIddict;

public class OpenIddictApplicationDto
{
    public Guid Id { get; set; }
    public string ClientId { get; set; } = default!;
    public string? ClientType { get; set; }
    public string? ConsentType { get; set; }
    public string? DisplayName { get; set; }
    public string? ClientUri { get; set; }
    public List<string> RedirectUris { get; set; } = [];
    public List<string> PostLogoutRedirectUris { get; set; } = [];
    public List<string> GrantTypes { get; set; } = [];
    public List<string> Scopes { get; set; } = [];
}
