using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Picturepark.SDK.V1;
using Picturepark.SDK.V1.Authentication;
using Picturepark.SDK.V1.Contract;

namespace Picturepark.ServiceProvider.Example.BusinessProcess
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            await new HostBuilder()
                .ConfigureLogging(
                    (ctx, logging) =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    })
                .ConfigureAppConfiguration(
                    (ctx, app) =>
                    {
                        app.SetBasePath(Directory.GetCurrentDirectory());
                        app.AddJsonFile("appsettings.json", optional: true);
                        app.AddCommandLine(args);
                    })
                .ConfigureServices(
                    (ctx, services) =>
                    {
                        services.AddOptions();
                        services.Configure<SampleConfiguration>(ctx.Configuration.GetSection("config"));

                        services.AddLogging();

                        services.AddHostedService<ContentBatchDownloadService>();
                        services.AddHostedService<LiveStreamSubscriber>();

                        services.AddSingleton<IApplicationEventHandlerFactory, ApplicationEventHandlerFactory>();
                        services.AddTransient<IApplicationEventHandler, BusinessRuleFiredEventHandler>();
                        services.AddTransient<IApplicationEventHandler, BusinessProcessCancellationRequestedEventHandler>();

                        services.AddSingleton<ContentIdQueue>();

                        services.AddSingleton<Func<IPictureparkService>>(
                            s =>
                            {
                                return () =>
                                {
                                    var config = s.GetRequiredService<IOptions<SampleConfiguration>>();

                                    var authClient = new AccessTokenAuthClient(config.Value.ApiUrl, config.Value.AccessToken, config.Value.CustomerAlias);
                                    var client = new PictureparkService(new PictureparkServiceSettings(authClient));

                                    return client;
                                };
                            });

                        services.AddSingleton<IBusinessProcessCancellationManager, BusinessProcessCancellationManager>();
                    })
                .UseConsoleLifetime()
                .RunConsoleAsync().ConfigureAwait(false);
        }
    }
}