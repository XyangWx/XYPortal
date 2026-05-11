using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider;

[DependsOn(
    typeof(RandomStringProviderApplicationModule),
    typeof(RandomStringProviderDomainTestModule)
    )]
public class RandomStringProviderApplicationTestModule : AbpModule
{

}
