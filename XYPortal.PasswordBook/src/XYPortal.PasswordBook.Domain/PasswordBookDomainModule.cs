using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using XYPortal.RandomStringProvider;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(PasswordBookDomainSharedModule),
    typeof(RandomStringProviderDomainModule)
)]
public class PasswordBookDomainModule : AbpModule
{
    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        LoggerHelper.SetFactory(context.ServiceProvider.GetService<ILoggerFactory>());
        base.OnApplicationInitialization(context);
    }
}
