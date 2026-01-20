using XYPortal.MongoDB;
using XYPortal.Samples;
using Xunit;

namespace XYPortal.MongoDb.Applications;

[Collection(XYPortalTestConsts.CollectionDefinitionName)]
public class MongoDBSampleAppServiceTests : SampleAppServiceTests<XYPortalMongoDbTestModule>
{

}
