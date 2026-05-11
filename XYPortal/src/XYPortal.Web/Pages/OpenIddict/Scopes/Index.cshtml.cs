using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.OpenIddict;
using XYPortal.Permissions;

namespace XYPortal.Web.Pages.OpenIddict.Scopes;

[Authorize(XYPortalPermissions.OpenIdDictScopeManager)]
public class IndexModel : XYPortalPageModel
{
    private readonly IOpenIddictScopeAppService _appService;

    public IndexModel(IOpenIddictScopeAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetOpenIddictScopeListInput input)
    {
        var result = await _appService.GetListAsync(input);
        return new JsonResult(result);
    }

    public async Task<IActionResult> OnPostDeleteAsync(Guid id)
    {
        await _appService.DeleteAsync(id);
        return new NoContentResult();
    }
}
