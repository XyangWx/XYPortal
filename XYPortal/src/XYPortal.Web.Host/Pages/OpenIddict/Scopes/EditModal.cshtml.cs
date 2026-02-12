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

[Authorize(XYPortalPermissions.OpenIdDictScopeEdit)]
public class EditModalModel : XYPortalPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public EditViewModel Input { get; set; } = new();

    private readonly IOpenIddictScopeAppService _appService;

    public EditModalModel(IOpenIddictScopeAppService appService)
    {
        _appService = appService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _appService.GetAsync(Id);
        Input = new EditViewModel
        {
            Name = dto.Name,
            DisplayName = dto.DisplayName ?? string.Empty,
            Description = dto.Description ?? string.Empty,
            Resources = dto.Resources.Count > 0 ? string.Join("\n", dto.Resources) : null
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var dto = new UpdateOpenIddictScopeDto
        {
            DisplayName = Input.DisplayName,
            Description = Input.Description,
            Resources = SplitLines(Input.Resources)
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
        public string Name { get; set; } = default!;

        public string? DisplayName { get; set; }

        public string? Description { get; set; }

        public string? Resources { get; set; }
    }
}
