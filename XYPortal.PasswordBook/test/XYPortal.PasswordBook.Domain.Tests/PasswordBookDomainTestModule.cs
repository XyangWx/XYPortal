using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookDomainModule),
    typeof(PasswordBookTestBaseModule)
)]
public class PasswordBookDomainTestModule : AbpModule
{

}
