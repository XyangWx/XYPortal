using Volo.Abp.Modularity;

namespace XYPortal.LinkBoard;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class LinkBoardDomainTestBase<TStartupModule> : LinkBoardTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
