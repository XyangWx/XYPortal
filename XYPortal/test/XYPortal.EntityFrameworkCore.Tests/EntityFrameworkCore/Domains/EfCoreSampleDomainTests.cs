using XYPortal.Samples;
using Xunit;

namespace XYPortal.EntityFrameworkCore.Domains;

[Collection(XYPortalTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<XYPortalEntityFrameworkCoreTestModule>
{

}
