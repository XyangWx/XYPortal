using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp;

namespace XYPortal.RandomStringProvider.Samples;

[Area(RandomStringProviderRemoteServiceConsts.ModuleName)]
[RemoteService(Name = RandomStringProviderRemoteServiceConsts.RemoteServiceName)]
[Route("api/random-string-provider/example")]
public class ExampleController : RandomStringProviderController, ISampleAppService
{
    private readonly ISampleAppService _sampleAppService;

    public ExampleController(ISampleAppService sampleAppService)
    {
        _sampleAppService = sampleAppService;
    }

    [HttpGet]
    public async Task<SampleDto> GetAsync()
    {
        return await _sampleAppService.GetAsync();
    }

    [HttpGet]
    [Route("authorized")]
    [Authorize]
    public async Task<SampleDto> GetAuthorizedAsync()
    {
        return await _sampleAppService.GetAsync();
    }
}
