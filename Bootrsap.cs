using Tavu.Exceptions;
using Tavu.Storage;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.MicrosoftAccount;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Swashbuckle.AspNetCore.SwaggerUI;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.OAuth;

namespace Tavu
{
    public static class Bootstrap
    {
        public static void ConfigureAuthntication(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = MicrosoftAccountDefaults.AuthenticationScheme;
            })
            .AddCookie()
            .AddMicrosoftAccount(o =>
            {
                o.ClientId = configuration["Authentication:ClientId"] ?? throw new TavuServiceConfigurationException();
                o.ClientSecret = configuration["Authentication:ClientSecret"] ?? throw new TavuServiceConfigurationException();
                o.Events = new OAuthEvents{
                    OnRedirectToAuthorizationEndpoint = ctx =>
                    {
                        if (string.Equals(ctx.HttpContext.Request.Path, "/login", StringComparison.InvariantCultureIgnoreCase))
                        {
                            ctx.HttpContext.Response.Redirect(ctx.RedirectUri);
                        }
                        else 
                        {
                            ctx.HttpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        }
                            return Task.CompletedTask;

                    }
                };
            });
        }

        public static void ConfigureAzureStorage(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<AzureStorageOptions>(configuration.GetSection(AzureStorageOptions.AzureStorage));
            services.AddSingleton<IBlobStore, AzureStorageBlobStore>();
        }

        public static void ConfigureExcerciseStore(this IServiceCollection services)
        {
            services.AddScoped<IExcerciseStore, ExcerciseStore>();
        }
    }
}