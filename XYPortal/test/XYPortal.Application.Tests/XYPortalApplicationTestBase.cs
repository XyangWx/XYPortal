using Volo.Abp.Modularity;

namespace XYPortal;

public abstract class XYPortalApplicationTestBase<TStartupModule> : XYPortalTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
