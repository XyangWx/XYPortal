using XYPortal.RandomStringProvider.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal.RandomStringProvider;

public abstract class RandomStringProviderAppService : ApplicationService
{
    protected RandomStringProviderAppService()
    {
        LocalizationResource = typeof(RandomStringProviderResource);
        ObjectMapperContext = typeof(RandomStringProviderApplicationModule);
    }
}
