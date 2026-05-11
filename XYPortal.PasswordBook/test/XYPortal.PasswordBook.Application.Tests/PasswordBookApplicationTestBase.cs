using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class PasswordBookApplicationTestBase<TStartupModule> : PasswordBookTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
