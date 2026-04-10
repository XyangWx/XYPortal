using XYPortal.PasswordBook.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace XYPortal.PasswordBook;

public abstract class PasswordBookController : AbpControllerBase
{
    protected PasswordBookController()
    {
        LocalizationResource = typeof(PasswordBookResource);
    }
}
