using Volo.Abp.Modularity;

namespace XYPortal.PasswordBook;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class PasswordBookDomainTestBase<TStartupModule> : PasswordBookTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
