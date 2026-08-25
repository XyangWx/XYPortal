using Volo.Abp.Modularity;

namespace XYPortal.EvGRPC;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class EvGRPCDomainTestBase<TStartupModule> : EvGRPCTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
