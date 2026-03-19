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

}
