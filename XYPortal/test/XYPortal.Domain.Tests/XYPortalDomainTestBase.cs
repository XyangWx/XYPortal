using Volo.Abp.Modularity;

namespace XYPortal;

/* Inherit from this class for your domain layer tests. */
public abstract class XYPortalDomainTestBase<TStartupModule> : XYPortalTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
