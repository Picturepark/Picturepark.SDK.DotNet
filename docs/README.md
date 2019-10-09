# Getting started

## Resources
Please refer to [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/index.html) for detailed specification.

## Usage in .NET

Install the Picturepark SDK NuGet package in your .NET project (supports .NET 4.5+ and .NET Standard 1.3+): 

    Install-Package Picturepark.SDK.V1
    
Create a new `PictureparkService` instance and access remote Picturepark server as follows: 

```csharp
var authClient = new AccessTokenAuthClient("https://api.mypcpserver.com", "AccessToken", "CustomerAlias");
var settings = new PictureparkServiceSettings(authClient);
using (var client = new PictureparkService(settings))
{
    var content = await client.Content.GetAsync("myContentId");
}
```

Note: The default constructor adds a retry handler for the HTTP 429 status code (Too many requests) 
and will automatically retry up to 3 times when the client gets throttled.

To change this behavior, create the client as follows:
```csharp
var handler = new PictureparkRetryHandler();
var httpClient = new HttpClient(handler) { Timeout = settings.HttpTimeout };
using (var client = new PictureparkService(settings, httpClient))
{
}
```

### Usage in ASP.NET Core

Register the Picturepark .NET service classes in the ASP.NET Core dependency injection system (`Startup.cs`): 

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddApplicationInsightsTelemetry(Configuration);
    services.AddMvc();

    services.AddScoped<IPictureparkService, PictureparkService>();
    services.AddSingleton<IPictureparkServiceSettings>(new PictureparkServiceSettings(
        new AccessTokenAuthClient("https://api.server.com", "MyAccessToken", "MyCustomerAlias")));
}
```

Inject `IPictureparkService` into your controller: 

```csharp
public class MyController : Controller
{
    private readonly IPictureparkService _pictureparkService;

    public MyController(IPictureparkService pictureparkService)
    {
        _pictureparkService = pictureparkService;
    }
    
    ...
```

To add handling for HTTP 429 (Too many requests) responses, it is recommended to use the `HttpClientFactory` available since ASP.NET Core 2.1: 

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IPictureparkService, PictureparkService>();
    services.AddSingleton<IPictureparkServiceSettings>(new PictureparkServiceSettings(
        new AccessTokenAuthClient("https://api.server.com", "MyAccessToken", "MyCustomerAlias")));

    services.AddHttpClient<IPictureparkService, PictureparkService>()
        .ConfigurePrimaryHttpMessageHandler(() => new PictureparkRetryHandler());
}
```

See [this article](https://www.stevejgordon.co.uk/introduction-to-httpclientfactory-aspnetcore) for more information.

### Usage with .NET 4.5.x framework

When installing the SDK in .NET 4.5.x targets you need to globally enable TLS 1.2: 

```csharp
ServicePointManager.SecurityProtocol = 
    SecurityProtocolType.Ssl3 | 
    SecurityProtocolType.Tls12 | 
    SecurityProtocolType.Tls11 | 
    SecurityProtocolType.Tls;
```
