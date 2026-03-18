using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook;

[DependsOn(
    typeof(PasswordBookApplicationModule),
    typeof(PasswordBookDomainTestModule)
    )]
public class PasswordBookApplicationTestModule : AbpModule
{

}
