using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace XYPortal.Pages;

public class Index_Tests : XYPortalWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
