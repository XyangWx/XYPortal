using XYPortal.Samples;
using Xunit;

namespace XYPortal.MongoDB.Domains;

[Collection(XYPortalTestConsts.CollectionDefinitionName)]
public class MongoDBSampleDomainTests : SampleDomainTests<XYPortalMongoDbTestModule>
{

}
