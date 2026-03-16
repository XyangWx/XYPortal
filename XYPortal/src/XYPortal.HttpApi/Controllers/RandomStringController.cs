using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;
using XYPortal.Controllers;
using XYPortal.RandomStringProvider.RandomStringProvider;

namespace XYPortal.HttpApi.Controllers;

[RemoteService(Name = "Default")]
[Area("app")]
[Route("api/app/random-string")]
public class RandomStringController : XYPortalController, IRandomStringApplication
{
    private readonly IRandomStringApplication _randomStringAppService;

    public RandomStringController(IRandomStringApplication randomStringAppService)
    {
        _randomStringAppService = randomStringAppService;
    }

    [HttpPost("make")]
    public virtual string MakeRandomString([FromBody] RandomStringInput input)
    {
        return _randomStringAppService.MakeRandomString(input);
    }

    [HttpPost("make-async")]
    public virtual Task<string> MakeRandomStringAsync([FromBody] RandomStringInput input)
    {
        return _randomStringAppService.MakeRandomStringAsync(input);
    }
}
