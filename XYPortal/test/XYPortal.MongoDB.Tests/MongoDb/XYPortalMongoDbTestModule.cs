using System;
using Volo.Abp.Data;
using Volo.Abp.Modularity;

namespace XYPortal.MongoDB;

[DependsOn(
    typeof(XYPortalApplicationTestModule),
    typeof(XYPortalMongoDbModule)
)]
public class XYPortalMongoDbTestModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpDbConnectionOptions>(options =>
        {
            options.ConnectionStrings.Default = XYPortalMongoDbFixture.GetRandomConnectionString();
        });
    }
}
