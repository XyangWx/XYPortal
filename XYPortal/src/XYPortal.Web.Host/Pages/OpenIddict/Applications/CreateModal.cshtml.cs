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

[Authorize(XYPortalPermissions.OpenIdDictApplicationCreate)]
public class CreateModalModel : XYPortalPageModel
{
    [BindProperty]
    public CreateViewModel Input { get; set; } = new();

    private readonly IOpenIddictApplicationAppService _appService;

    public CreateModalModel(IOpenIddictApplicationAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
        Input = new CreateViewModel();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new CreateOpenIddictApplicationDto
        {
            ClientId = Input.ClientId,
            ClientType = Input.ClientType,
            ConsentType = Input.ConsentType,
            DisplayName = Input.DisplayName,
            ClientSecret = Input.ClientSecret,
            ClientUri = Input.ClientUri,
            GrantTypes = Input.GrantTypes ?? [],
            Scopes = SplitLines(Input.Scopes),
            RedirectUris = SplitLines(Input.RedirectUris),
            PostLogoutRedirectUris = SplitLines(Input.PostLogoutRedirectUris)
        };

        await _appService.CreateAsync(dto);
        return NoContent();
    }

    private static List<string> SplitLines(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return [];
        return value.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).ToList();
    }

    public class CreateViewModel
    {
        [Required]
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

        public string? Scopes { get; set; }

        public string? RedirectUris { get; set; }

        public string? PostLogoutRedirectUris { get; set; }
    }
}
