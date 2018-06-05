using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Picturepark.Microsite.PressPortal.Configuration;
using Picturepark.Microsite.PressPortal.Helpers;
using Picturepark.Microsite.PressPortal.Repository;
using Picturepark.Microsite.PressPortal.Services;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Picturepark.Microsite.PressPortal.Contracts;

namespace Picturepark.Microsite.PressPortal
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure Picturepark
            services.Configure<PictureparkConfiguration>(Configuration.GetSection("Picturepark"));

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Register PictureparkServiceClient as singleton
            services.AddSingleton<IPictureparkServiceClientSettings, PictureparkServiceClientSettings>();
            services.AddSingleton<IPictureparkServiceClient, PictureparkServiceClient>();

            // Register PictureparkPerRequestClient as transient
            services.AddTransient<IPictureparkPerRequestClientSettings, PictureparkPerRequestClientSettings>();
            services.AddTransient<IPictureparkPerRequestClient, PictureparkPerRequestClient>();

            services.AddSingleton<IServiceHelper, ServiceHelper>();
            services.AddSingleton<IPressReleaseRepository, PressReleaseRepository>();

            // Add the localization services to the services container
            services.AddLocalization(options => options.ResourcesPath = "Resources");

            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1)
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            // Configure authentication
            services.AddAuthentication("Cookies")
                .AddCookie("Cookies", options =>
                {
                    options.LoginPath = "/account/login";
                    options.AccessDeniedPath = "/account/denied";
                })
                .AddOpenIdConnect("oidc", options =>
                {
                    // Read the authentication settings
                    var authConfig = new AuthenticationConfiguration();
                    Configuration.GetSection("Authentication").Bind(authConfig);

                    options.SignInScheme = "Cookies";

                    options.Authority = authConfig.Authority;
                    options.ClientId = authConfig.ClientId;
                    options.ClientSecret = authConfig.ClientSecret;
                    options.ResponseType = "code id_token";

                    // Development only setting
                    options.RequireHttpsMetadata = false;

                    options.SaveTokens = true;
                    options.UseTokenLifetime = true;
                    options.GetClaimsFromUserInfoEndpoint = true;

                    options.Events.OnRedirectToIdentityProvider = context =>
                    {
                        var tenant = new { id = authConfig.CustomerId, alias = authConfig.CustomerAlias };
                        context.ProtocolMessage.SetParameter("acr_values", "tenant:" + JsonConvert.SerializeObject(tenant));
                        return Task.FromResult(0);
                    };

                    options.Events.OnTicketReceived = context =>
                    {
                        context.Properties.IsPersistent = true;
                        return Task.FromResult(0);
                    };

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
