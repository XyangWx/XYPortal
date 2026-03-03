using XYPortal.RandomStringProvider.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.RandomStringProvider;

public abstract class RandomStringProviderController : AbpControllerBase
{
    protected RandomStringProviderController()
    {
        LocalizationResource = typeof(RandomStringProviderResource);
    }
}
