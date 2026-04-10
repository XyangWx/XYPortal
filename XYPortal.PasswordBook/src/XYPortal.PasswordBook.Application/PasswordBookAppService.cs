using XYPortal.PasswordBook.Localization;
using Volo.Abp.Application.Services;

namespace XYPortal.PasswordBook;

public abstract class PasswordBookAppService : ApplicationService
{
    protected PasswordBookAppService()
    {
        LocalizationResource = typeof(PasswordBookResource);
        ObjectMapperContext = typeof(PasswordBookApplicationModule);
    }
}
