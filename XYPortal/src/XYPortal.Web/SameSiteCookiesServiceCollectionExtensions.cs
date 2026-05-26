using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace XYPortal.Web
{
	public static class SameSiteCookiesServiceCollectionExtensions
	{
		private static bool _useSameSitePolicy;

		public static IServiceCollection AddSameSiteCookiePolicy(this IServiceCollection services, bool useSameSitePolicy = false)
		{
			_useSameSitePolicy = useSameSitePolicy;

			services.Configure<CookiePolicyOptions>(options =>
			{
				options.MinimumSameSitePolicy = SameSiteMode.Unspecified;
				options.OnAppendCookie = cookieContext =>
					CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
				options.OnDeleteCookie = cookieContext =>
					CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
			});

			return services;
		}

		private static void CheckSameSite(HttpContext httpContext, CookieOptions options)
		{
			if (!_useSameSitePolicy)
			{
				return;
			}

			if (options.SameSite == SameSiteMode.None)
			{
				var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
				if (!httpContext.Request.IsHttps || DisallowsSameSiteNone(userAgent))
				{
					// For .NET Core < 3.1 set SameSite = (SameSiteMode)(-1)
					options.SameSite = SameSiteMode.Unspecified;
				}
			}
		}

		private static bool DisallowsSameSiteNone(string userAgent)
		{
			// Cover all iOS based browsers here. This includes:
			// - Safari on iOS 12 for iPhone, iPod Touch, iPad
			// - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
			// - Chrome on iOS 12 for iPhone, iPod Touch, iPad
			// All of which are broken by SameSite=None, because they use the iOS networking stack
			if (userAgent.Contains("CPU iPhone OS 12") || userAgent.Contains("iPad; CPU OS 12"))
			{
				return true;
			}

			// Cover Mac OS X based browsers that use the Mac OS networking stack. This includes:
			// - Safari on Mac OS X.
			// This does not include:
			// - Chrome on Mac OS X
			// Because they do not use the Mac OS networking stack.
			if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
				userAgent.Contains("Version/") && userAgent.Contains("Safari"))
			{
				return true;
			}

			// 🌟 核心修正：使用全局标准的正则，严格拦截 Chrome 50-69 版本，避免误伤 100+ 版本
			if (System.Text.RegularExpressions.Regex.IsMatch(userAgent, @"Chrome/((5[0-9])|(6[0-9]))\."))
			{
				return true;
			}

			return false;
		}
	}
}
