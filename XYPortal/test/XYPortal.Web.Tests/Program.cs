using Microsoft.AspNetCore.Builder;
using XYPortal;
using Volo.Abp.AspNetCore.TestBase;

var builder = WebApplication.CreateBuilder();

builder.Environment.ContentRootPath = GetWebProjectContentRootPathHelper.Get("XYPortal.Web.csproj");
await builder.RunAbpModuleAsync<XYPortalWebTestModule>(applicationName: "XYPortal.Web" );

public partial class Program
{
}
