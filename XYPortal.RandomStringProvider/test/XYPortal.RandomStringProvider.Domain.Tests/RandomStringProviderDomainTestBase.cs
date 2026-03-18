using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class RandomStringProviderDomainTestBase<TStartupModule> : RandomStringProviderTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
