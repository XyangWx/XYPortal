using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.OpenIddict;
using XYPortal.Permissions;

namespace XYPortal.Web.Pages.OpenIddict.Scopes;

[Authorize(XYPortalPermissions.OpenIdDictScopeCreate)]
public class CreateModalModel : XYPortalPageModel
{
    [BindProperty]
    public CreateViewModel Input { get; set; } = new();

    private readonly IOpenIddictScopeAppService _appService;

    public CreateModalModel(IOpenIddictScopeAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
        Input = new CreateViewModel();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new CreateOpenIddictScopeDto
        {
            Name = Input.Name,
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Resources = SplitLines(Input.Resources)
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
        public string Name { get; set; } = default!;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public string? Resources { get; set; }
    }
}
