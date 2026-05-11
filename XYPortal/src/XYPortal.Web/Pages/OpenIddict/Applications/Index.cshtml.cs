using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using XYPortal.OpenIddict;
using XYPortal.Permissions;

namespace XYPortal.Web.Pages.OpenIddict.Applications;

[Authorize(XYPortalPermissions.OpenIdDictApplicationManager)]
public class IndexModel : XYPortalPageModel
{
    private readonly IOpenIddictApplicationAppService _appService;

    public IndexModel(IOpenIddictApplicationAppService appService)
    {
        _appService = appService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetListAsync(GetOpenIddictApplicationListInput input)
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
