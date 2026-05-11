using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(RandomStringProviderDomainSharedModule)
)]
public class RandomStringProviderDomainModule : AbpModule
{

}
