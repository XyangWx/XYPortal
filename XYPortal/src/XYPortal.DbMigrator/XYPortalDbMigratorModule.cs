using XYPortal.EntityFrameworkCore;
using Volo.Abp.Autofac;
using Volo.Abp.Modularity;

namespace XYPortal.DbMigrator;

[DependsOn(
    typeof(AbpAutofacModule),
    typeof(XYPortalEntityFrameworkCoreModule),
    typeof(XYPortalApplicationContractsModule)
    )]
public class XYPortalDbMigratorModule : AbpModule
{
}
