using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.OpenApi;
using StackExchange.Redis;
using System;
using System.IO;
using Volo.Abp;
using Volo.Abp.AspNetCore.Authentication.OpenIdConnect;
using Volo.Abp.AspNetCore.Mvc.Client;
using Volo.Abp.AspNetCore.Mvc.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.LeptonXLite.Bundling;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared;
using Volo.Abp.AspNetCore.Mvc.UI.Theme.Shared.Toolbars;
using Volo.Abp.AspNetCore.Serilog;
using Volo.Abp.Autofac;
using Volo.Abp.Caching;
using Volo.Abp.Caching.StackExchangeRedis;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Http.Client.IdentityModel.Web;
using Volo.Abp.Http.Client.Web;
using Volo.Abp.Identity.Web;
using Volo.Abp.Modularity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Volo.Abp.SettingManagement.Web;
using Volo.Abp.Swashbuckle;
using Volo.Abp.TenantManagement.Web;
using Volo.Abp.UI.Navigation;
using Volo.Abp.UI.Navigation.Urls;
using Volo.Abp.VirtualFileSystem;
using XYPortal.LinkBoard;
using XYPortal.Localization;
using XYPortal.MultiTenancy;
using XYPortal.PasswordBook.Web;
using XYPortal.RandomStringProvider;
using XYPortal.RandomStringProvider.Web;
using XYPortal.Web.Menus;

namespace XYPortal.Web;

[DependsOn(
    typeof(XYPortalHttpApiClientModule),
    typeof(XYPortalHttpApiModule),
    typeof(LinkBoardHttpApiClientModule),
    typeof(PasswordBookWebModule),
    typeof(AbpAspNetCoreAuthenticationOpenIdConnectModule),
    typeof(AbpAspNetCoreMvcClientModule),
    typeof(AbpHttpClientWebModule),
    typeof(AbpAspNetCoreMvcUiLeptonXLiteThemeModule),
    typeof(AbpAutofacModule),
    typeof(AbpCachingStackExchangeRedisModule),
    typeof(AbpDistributedLockingModule),
    typeof(AbpSettingManagementWebModule),
    typeof(AbpHttpClientIdentityModelWebModule),
    typeof(AbpIdentityWebModule),
    typeof(AbpTenantManagementWebModule),
    typeof(AbpAspNetCoreSerilogModule),
    typeof(AbpSwashbuckleModule),
    typeof(RandomStringProviderWebModule),
    typeof(RandomStringProviderApplicationModule)
    )]
public class XYPortalWebModule : AbpModule
{
    public override void PreConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.PreConfigure<AbpMvcDataAnnotationsLocalizationOptions>(options =>
        {
            options.AddAssemblyResource(
                typeof(XYPortalResource),
                typeof(XYPortalDomainSharedModule).Assembly,
                typeof(XYPortalApplicationContractsModule).Assembly,
                typeof(XYPortalWebModule).Assembly
            );
        });
    }

    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var hostingEnvironment = context.Services.GetHostingEnvironment();
        var configuration = context.Services.GetConfiguration();

        if (!configuration.GetValue<bool>("App:RequireHttps"))
        {
            context.Services.AddSameSiteCookiePolicy(configuration.GetValue<bool>("App:UseSameSitePolicy", false));
        }

        ConfigureBundles();
        ConfigureCache();
        ConfigureDataProtection(context, configuration, hostingEnvironment);
        ConfigureDistributedLocking(context, configuration);
        ConfigureUrls(configuration);
        ConfigureAuthentication(context, configuration);
        ConfigureVirtualFileSystem(hostingEnvironment);
        ConfigureNavigationServices(configuration);
        ConfigureMultiTenancy();
        ConfigureSwaggerServices(context.Services);

        context.Services.AddMapperlyObjectMapper<XYPortalWebModule>();



		context.Services.AddLogging(
			builder =>
			{
				builder.ClearProviders();

#if DEBUG
				builder.SetMinimumLevel(LogLevel.Debug);
#else
                builder.SetMinimumLevel(LogLevel.Warning);
#endif

				builder.AddLog4Net("log4net.xml", true);
			});
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

    private void ConfigureCache()
    {
        Configure<AbpDistributedCacheOptions>(options =>
        {
            options.KeyPrefix = "XYPortal:";
        });
    }

    private void ConfigureUrls(IConfiguration configuration)
    {
        Configure<AppUrlOptions>(options =>
        {
            options.Applications["MVC"].RootUrl = configuration["App:SelfUrl"];
        });
    }

    private void ConfigureMultiTenancy()
    {
        Configure<AbpMultiTenancyOptions>(options =>
        {
            options.IsEnabled = MultiTenancyConsts.IsEnabled;
        });
    }

    private void ConfigureAuthentication(ServiceConfigurationContext context, IConfiguration configuration)
    {
        context.Services.AddAuthentication(options =>
            {
                options.DefaultScheme = "Cookies";
                options.DefaultChallengeScheme = "oidc";
            })
            .AddCookie("Cookies", options =>
            {
                options.ExpireTimeSpan = TimeSpan.FromDays(365);
                options.CheckTokenExpiration();
            })
            .AddAbpOpenIdConnect("oidc", options =>
            {
                options.Authority = configuration["AuthServer:Authority"];
                options.RequireHttpsMetadata = configuration.GetValue<bool>("AuthServer:RequireHttpsMetadata");
                options.ResponseType = OpenIdConnectResponseType.CodeIdToken;

                options.ClientId = configuration["AuthServer:ClientId"];
                options.ClientSecret = configuration["AuthServer:ClientSecret"];

                options.UsePkce = true;
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;

                options.Scope.Add("roles");
                options.Scope.Add("email");
                options.Scope.Add("phone");
                options.Scope.Add("XYPortal");
            });
            /*
            * This configuration is used when the AuthServer is running on the internal network such as docker or k8s.
            * Configuring the redirecting URLs for internal network and the web
            * The login and the logout URLs are configured to redirect to the AuthServer real DNS for browser.
            * The token acquired and validated from the the internal network AuthServer URL.
            */
            if (configuration.GetValue<bool>("AuthServer:IsContainerized"))
            {
                context.Services.Configure<OpenIdConnectOptions>("oidc", options =>
                {
                    options.TokenValidationParameters.ValidIssuers = new[]
                    {
                        configuration["AuthServer:MetaAddress"]!.EnsureEndsWith('/'),
                        configuration["AuthServer:Authority"]!.EnsureEndsWith('/')
                    };

                    options.MetadataAddress = configuration["AuthServer:MetaAddress"]!.EnsureEndsWith('/') +
                                            ".well-known/openid-configuration";

                    var previousOnRedirectToIdentityProvider = options.Events.OnRedirectToIdentityProvider;
                    options.Events.OnRedirectToIdentityProvider = async ctx =>
                    {
                        // Intercept the redirection so the browser navigates to the right URL in your host
                        ctx.ProtocolMessage.IssuerAddress = configuration["AuthServer:Authority"]!.EnsureEndsWith('/') + "connect/authorize";

                        if (previousOnRedirectToIdentityProvider != null)
                        {
                            await previousOnRedirectToIdentityProvider(ctx);
                        }
                    };
                    var previousOnRedirectToIdentityProviderForSignOut = options.Events.OnRedirectToIdentityProviderForSignOut;
                    options.Events.OnRedirectToIdentityProviderForSignOut = async ctx =>
                    {
                        // Intercept the redirection for signout so the browser navigates to the right URL in your host
                        ctx.ProtocolMessage.IssuerAddress = configuration["AuthServer:Authority"]!.EnsureEndsWith('/') + "connect/logout";

                        if (previousOnRedirectToIdentityProviderForSignOut != null)
                        {
                            await previousOnRedirectToIdentityProviderForSignOut(ctx);
                        }
                    };
                });
            }

        context.Services.Configure<AbpClaimsPrincipalFactoryOptions>(options =>
        {
            options.IsDynamicClaimsEnabled = true;
        });
    }

    private void ConfigureVirtualFileSystem(IWebHostEnvironment hostingEnvironment)
    {
        if (hostingEnvironment.IsDevelopment())
        {
            Configure<AbpVirtualFileSystemOptions>(options =>
            {
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalDomainSharedModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Domain.Shared"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalApplicationContractsModule>(Path.Combine(hostingEnvironment.ContentRootPath, $"..{Path.DirectorySeparatorChar}XYPortal.Application.Contracts"));
                options.FileSets.ReplaceEmbeddedByPhysical<XYPortalWebModule>(hostingEnvironment.ContentRootPath);
            });
        }
    }

    private void ConfigureNavigationServices(IConfiguration configuration)
    {
        Configure<AbpNavigationOptions>(options =>
        {
            options.MenuContributors.Add(new XYPortalMenuContributor(configuration));
        });

        Configure<AbpToolbarOptions>(options =>
        {
            options.Contributors.Add(new XYPortalToolbarContributor());
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

    private void ConfigureDataProtection(
        ServiceConfigurationContext context,
        IConfiguration configuration,
        IWebHostEnvironment hostingEnvironment)
    {
        var dataProtectionBuilder = context.Services.AddDataProtection().SetApplicationName("XYPortal");
        if (!hostingEnvironment.IsDevelopment())
        {
            var redis = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            dataProtectionBuilder.PersistKeysToStackExchangeRedis(redis, "XYPortal-Protection-Keys");
        }
    }

    private void ConfigureDistributedLocking(
        ServiceConfigurationContext context,
        IConfiguration configuration)
    {
        context.Services.AddSingleton<IDistributedLockProvider>(sp =>
        {
            var connection = ConnectionMultiplexer.Connect(configuration["Redis:Configuration"]!);
            return new RedisDistributedSynchronizationProvider(connection.GetDatabase());
        });
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

        if (IsMultiTenancyEnabled())
        {
            app.UseMultiTenancy();
        }

        app.UseDynamicClaims();
        app.UseAuthorization();
        app.UseSwagger();
        app.UseAbpSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "XYPortal API");
        });
        app.UseAbpSerilogEnrichers();
        app.UseConfiguredEndpoints();
    }
    
    private static bool IsMultiTenancyEnabled()
    {
        return MultiTenancyConsts.IsEnabled;
    }
}
