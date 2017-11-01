using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Picturepark.Microsite.Example.Configuration;
using Microsoft.Extensions.Options;
using Picturepark.Microsite.Example.Helpers;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Picturepark.Microsite.Example.Repository;
using Picturepark.Microsite.Example.Contracts;
using Picturepark.Microsite.Example.Services;

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
			// Configure Picturepark
			services.Configure<PictureparkConfiguration>(Configuration.GetSection("Picturepark"));

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
					options.GetClaimsFromUserInfoEndpoint = true;

					options.Events.OnRedirectToIdentityProvider = context =>
					{
						var tenant = new {id = authConfig.CustomerId, alias = authConfig.CustomerAlias};
						context.ProtocolMessage.SetParameter("acr_values", "tenant:" + JsonConvert.SerializeObject(tenant) );
						return Task.FromResult(0);
					};

					options.Scope.Clear();
					foreach (var scope in authConfig.Scopes) {
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

		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			var locOptions = app.ApplicationServices.GetService<IOptions<RequestLocalizationOptions>>();
			app.UseRequestLocalization(locOptions.Value);

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseBrowserLink();
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

			serviceHelper.EnsureSchemaExists<PressRelease>(null, updateSchema);

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Overview}/{id?}");
			});
		}
	}
}
