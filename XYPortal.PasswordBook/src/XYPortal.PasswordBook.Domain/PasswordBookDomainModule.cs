using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(PasswordBookDomainSharedModule)
)]
public class PasswordBookDomainModule : AbpModule
{

}
