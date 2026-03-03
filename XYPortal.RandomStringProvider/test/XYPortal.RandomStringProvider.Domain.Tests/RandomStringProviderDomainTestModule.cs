using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderDomainModule),
    typeof(RandomStringProviderTestBaseModule)
)]
public class RandomStringProviderDomainTestModule : AbpModule
{

}
