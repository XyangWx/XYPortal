using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XYPortal.OpenIddict;

public class CreateOpenIddictApplicationDto
{
    [Required]
    public string ClientId { get; set; } = default!;

    [Required]
    public string ClientType { get; set; } = default!;

    [Required]
    public string ConsentType { get; set; } = default!;

    [Required]
    public string DisplayName { get; set; } = default!;

    public string? ClientSecret { get; set; }

    public string? ClientUri { get; set; }

    [Required]
    public List<string> GrantTypes { get; set; } = [];

    [Required]
    public List<string> Scopes { get; set; } = [];

    public List<string>? RedirectUris { get; set; }

    public List<string>? PostLogoutRedirectUris { get; set; }
}
