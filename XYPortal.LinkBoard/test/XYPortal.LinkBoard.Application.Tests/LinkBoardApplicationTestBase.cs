using Volo.Abp.Modularity;

namespace XYPortal.LinkBoard;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class LinkBoardApplicationTestBase<TStartupModule> : LinkBoardTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
