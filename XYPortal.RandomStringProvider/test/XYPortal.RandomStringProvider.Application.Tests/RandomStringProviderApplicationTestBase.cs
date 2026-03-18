using Volo.Abp.Modularity;

namespace XYPortal.RandomStringProvider;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class RandomStringProviderApplicationTestBase<TStartupModule> : RandomStringProviderTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
