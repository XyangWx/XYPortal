using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.OpenIddict;
using XYPortal.Permissions;

namespace XYPortal.Web.Pages.OpenIddict.Applications;

[Authorize(XYPortalPermissions.OpenIdDictApplicationEdit)]
public class EditModalModel : XYPortalPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EditViewModel Input { get; set; } = new();

    public List<OpenIddictScopeDto> AvailableScopes { get; set; } = [];

    private readonly IOpenIddictApplicationAppService _appService;
    private readonly IOpenIddictScopeAppService _scopeAppService;

    public EditModalModel(IOpenIddictApplicationAppService appService, IOpenIddictScopeAppService scopeAppService)
    {
        _appService = appService;
        _scopeAppService = scopeAppService;
    }

    private static readonly string[] WellKnownScopes =
        ["openid", "profile", "email", "phone", "address", "roles"];

    public async Task OnGetAsync()
    {
        var dto = await _appService.GetAsync(Id);
        Input = new EditViewModel
        {
            ClientId = dto.ClientId,
            ClientType = dto.ClientType ?? "public",
            ConsentType = dto.ConsentType ?? "explicit",
            DisplayName = dto.DisplayName ?? string.Empty,
            ClientUri = dto.ClientUri,
            GrantTypes = dto.GrantTypes,
            Scopes = dto.Scopes,
            RedirectUris = dto.RedirectUris.Count > 0 ? string.Join("\n", dto.RedirectUris) : null,
            PostLogoutRedirectUris = dto.PostLogoutRedirectUris.Count > 0 ? string.Join("\n", dto.PostLogoutRedirectUris) : null
        };

        var scopes = await _scopeAppService.GetListAsync(new GetOpenIddictScopeListInput { MaxResultCount = 1000 });
        var dbScopes = scopes.Items.ToList();

        AvailableScopes = WellKnownScopes
            .Where(s => dbScopes.All(d => d.Name != s))
            .Select(s => new OpenIddictScopeDto { Name = s })
            .Concat(dbScopes)
            .ToList();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new UpdateOpenIddictApplicationDto
        {
            ClientType = Input.ClientType,
            ConsentType = Input.ConsentType,
            DisplayName = Input.DisplayName,
            ClientSecret = Input.ClientSecret,
            ClientUri = Input.ClientUri,
            GrantTypes = Input.GrantTypes ?? [],
            Scopes = Input.Scopes ?? [],
            RedirectUris = SplitLines(Input.RedirectUris),
            PostLogoutRedirectUris = SplitLines(Input.PostLogoutRedirectUris)
        };

        await _appService.UpdateAsync(Id, dto);
        return NoContent();
    }

    private static List<string> SplitLines(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        return value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public class EditViewModel
    {
        public string ClientId { get; set; } = default!;

        [Required]
        public string ClientType { get; set; } = "public";

        [Required]
        public string ConsentType { get; set; } = "explicit";

        [Required]
        public string DisplayName { get; set; } = default!;

        public string? ClientSecret { get; set; }

        public string? ClientUri { get; set; }

        public List<string> GrantTypes { get; set; } = [];

        public List<string> Scopes { get; set; } = [];

        public string? RedirectUris { get; set; }

        public string? PostLogoutRedirectUris { get; set; }
    }
}
