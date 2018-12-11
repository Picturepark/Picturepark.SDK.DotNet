using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Picturepark.Microsite.Example.Configuration;
using Picturepark.Microsite.Example.Contracts;
using Picturepark.Microsite.Example.Controllers;
using Picturepark.Microsite.Example.Helpers;
using Picturepark.Microsite.Example.Repository;
using Picturepark.Microsite.Example.Services;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;

namespace Picturepark.Microsite.Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddConfiguration<PictureparkConfiguration>(Configuration);
            var authConfig = services.AddConfiguration<AuthenticationConfiguration>(Configuration);
            services.AddConfiguration<AuthorizationConfiguration>(Configuration);

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register PictureparkAccessTokenService as singleton
            services.AddSingleton<IPictureparkAccessTokenServiceSettings, PictureparkAccessTokenServiceSettings>();
            services.AddSingleton<IPictureparkAccessTokenService, PictureparkAccessTokenService>();

            // Register PictureparkPerRequestService as transient
            services.AddTransient<IPictureparkPerRequestServiceSettings, PictureparkPerRequestServiceSettings>();
            services.AddTransient<IPictureparkPerRequestService, PictureparkPerRequestService>();

            services.AddSingleton<IServiceHelper, ServiceHelper>();
            services.AddSingleton<IPressReleaseRepository, PressReleaseRepository>();

            // Add the localization services to the services container
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // Configure authentication
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = AccountController.LoginPath;
                    options.AccessDeniedPath = "/account/denied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    options.SignInScheme = "Cookies";

                    options.Authority = authConfig.Authority;
                    options.ClientId = authConfig.ClientId;
                    options.ClientSecret = authConfig.ClientSecret;
                    options.ResponseType = "code id_token";

                    // Development only setting
                    options.RequireHttpsMetadata = false;

                    options.SaveTokens = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.EventsType = typeof(OidcEvents);

                    options.Scope.Clear();
                    foreach (var scope in authConfig.Scopes)
                    {
                        options.Scope.Add(scope);
                    }

                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        NameClaimType = "name",
                        RoleClaimType = "role"
                    };
                });

            // Configure supported cultures and localization options
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new[]
                {
                        new CultureInfo("en"),
                        new CultureInfo("de")
                };
                options.DefaultRequestCulture = new RequestCulture(culture: "en", uiCulture: "en");
                options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddTransient<OidcEvents>();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
            app.UseRequestLocalization(locOptions.Value);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();

            app.UseAuthentication();

            // Register / update schemas in PCP
            var serviceHelper = app.ApplicationServices.GetService<IServiceHelper>();
            var updateSchema = env.IsDevelopment();

            serviceHelper.EnsureSchemaExists<PressRelease>(null, updateSchema).Wait();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Overview}/{id?}");
            });
        }
    }
}
