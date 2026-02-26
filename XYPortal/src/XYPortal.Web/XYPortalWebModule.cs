using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using OpenIddict.Server.AspNetCore;
using OpenIddict.Validation.AspNetCore;
using System;
using System.IO;
using Volo.Abp;
using Volo.Abp.Account.Web;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.FeatureManagement;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.OpenIddict;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using XYPortal.EntityFrameworkCore;
using XYPortal.Localization;
using XYPortal.MultiTenancy;
using XYPortal.Web.Menus;

namespace XYPortal.Web;

[DependsOn(
    typeof(XYPortalHttpApiModule),
    typeof(XYPortalApplicationModule),
    typeof(XYPortalEntityFrameworkCoreModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpAccountWebOpenIddictModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpFeatureManagementWebModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule)
    )]
public class XYPortalWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(XYPortalResource),
                typeof(XYPortalDomainModule).Assembly,
                typeof(XYPortalDomainSharedModule).Assembly,
                typeof(XYPortalApplicationModule).Assembly,
                typeof(XYPortalApplicationContractsModule).Assembly,
                typeof(XYPortalWebModule).Assembly
            );
        });

        PreConfigure<OpenIddictBuilder>(builder =>
        {
            builder.AddValidation(options =>
            {
                options.AddAudiences("XYPortal");
                options.UseLocalServer();
                options.UseAspNetCore();
            });
        });

        // Disable HTTPS requirement for development/HTTP mode
        PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
        {
            serverBuilder.UseAspNetCore().DisableTransportSecurityRequirement();
        });

        if (!hostingEnvironment.IsDevelopment())
        {
            PreConfigure<AbpOpenIddictAspNetCoreOptions>(options =>
            {
                options.AddDevelopmentEncryptionAndSigningCertificate = false;
            });

            PreConfigure<OpenIddictServerBuilder>(serverBuilder =>
            {
                serverBuilder.AddProductionEncryptionAndSigningCertificate("openiddict.pfx", "0ee8f930-1356-4fd5-885f-c7713dc16053");
            });
        }
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

		if (!configuration.GetValue<bool>("App:RequireHttps"))
		{
			context.Services.AddSameSiteCookiePolicy();
		}

		ConfigureAuthentication(context);
        ConfigureUrls(configuration);
        ConfigureBundles();
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices();
        ConfigureAutoApiControllers();
        ConfigureSwaggerServices(context.Services);

        context.Services.AddMapperlyObjectMapper<XYPortalWebModule>();
    }

    private void CheckSameSite(HttpContext httpContext, CookieOptions options)
    {
        if (options.SameSite == SameSiteMode.None)
        {
            var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
            if (DisallowsSameSiteNone(userAgent))
            {
                options.SameSite = SameSiteMode.Unspecified;
            }
        }
    }

    private bool DisallowsSameSiteNone(string userAgent)
    {
        if (userAgent.Contains("CPU iPhone OS 12") ||
            userAgent.Contains("iPad; CPU OS 12"))
        {
            return true;
        }

        if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
            userAgent.Contains("Version/") && userAgent.Contains("Safari"))
        {
            return true;
        }

        if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
        {
            return true;
        }

        return false;
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context)
    {
        context.Services.ForwardIdentityAuthenticationForBearer(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);
        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });

        if (!configuration.GetValue<bool>("App:RequireHttps"))
        {
            Configure<OpenIddictServerAspNetCoreOptions>(
                options =>
                {
                    options.DisableTransportSecurityRequirement = true;
                });

            Configure<ForwardedHeadersOptions>(
                options =>
                {
                    options.ForwardedHeaders = ForwardedHeaders.XForwardedProto;
                });
        }
    }

    private void ConfigureBundles()
    {
        Configure<AbpBundlingOptions>(options =>
        {
            options.StyleBundles.Configure(
                LeptonXLiteThemeBundles.Styles.Global,
                bundle =>
                {
                    bundle.AddFiles("/global-styles.css");
                }
            );
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalDomainModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Domain"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalApplicationModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Application"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices()
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new XYPortalMenuContributor());
        });

		Configure<AbpToolbarOptions>(options =>
		{
			options.Contributors.Add(new XYPortalToolbarContributor());
		});
	}

    private void ConfigureAutoApiControllers()
    {
        Configure<AbpAspNetCoreMvcOptions>(options =>
        {
            options.ConventionalControllers.Create(typeof(XYPortalApplicationModule).Assembly);
        });
    }

    private void ConfigureSwaggerServices(IServiceCollection services)
    {
        services.AddAbpSwaggerGen(
            options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "XYPortal API", Version = "v1" });
                options.DocInclusionPredicate((docName, description) => true);
                options.CustomSchemaIds(type => type.FullName);
            }
        );
    }

    public override void OnApplicationInitialization(ApplicationInitializationContext context)
    {
        var app = context.GetApplicationBuilder();
        var env = context.GetEnvironment();
        var configuration = context.GetConfiguration();

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseAbpRequestLocalization();

        if (!env.IsDevelopment())
        {
            app.UseErrorPage();
        }

        if (!configuration.GetValue<bool>("App:RequireHttps"))
        {
            app.UseCookiePolicy(); // Add this line before UseCorrelationId
        }
        app.UseCorrelationId();
        app.MapAbpStaticAssets();
        app.UseRouting();
        app.UseAuthentication();
        app.UseAbpOpenIddictValidation();

        if (MultiTenancyConsts.IsEnabled)
        {
            app.UseMultiTenancy();
        }

        app.UseUnitOfWork();
        app.UseDynamicClaims();
        app.UseAuthorization();

        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "XYPortal API");
        });

        app.UseAuditing();
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
}
