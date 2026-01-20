using Xunit;

namespace XYPortal.EntityFrameworkCore;

[CollectionDefinition(XYPortalTestConsts.CollectionDefinitionName)]
public class XYPortalEntityFrameworkCoreCollection : ICollectionFixture<XYPortalEntityFrameworkCoreFixture>
{

}
