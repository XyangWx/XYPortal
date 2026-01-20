using XYPortal.Samples;
using Xunit;

namespace XYPortal.EntityFrameworkCore.Applications;

[Collection(XYPortalTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<XYPortalEntityFrameworkCoreTestModule>
{

}
