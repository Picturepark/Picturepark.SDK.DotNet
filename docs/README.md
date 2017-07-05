# SDK Documentation

**[API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/index.html)**

## Usage in C#

Install the Picturepark SDK NuGet package in your .NET project (supports .NET 4.5+ and .NET Standard 1.3+): 

    Install-Package Picturepark.SDK.V1
    
Create new `PictureparkClient` and access remote PCP server: 

```csharp
var authClient = new AccessTokenAuthClient("https://api.mypcpserver.com", "AccessToken", "CustomerAlias");
var settings = new PictureparkClientSettings(authClient);
using (var client = new PictureparkClient(settings))
{
    var content = await client.Contents.GetAsync("myContentId");
}
```

### Usage in ASP.NET Core

Register the Picturepark .NET service classes in the ASP.NET Core dependency injection system (`Startup.cs`): 

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddApplicationInsightsTelemetry(Configuration);
	services.AddMvc();

	services.AddSingleton<IPictureparkClientSettings>(
		new PictureparkClientSettings(new AccessTokenAuthClient("https://api.mypcpserver.com", "AccessToken", "CustomerAlias")));
	services.AddScoped<IPictureparkClient, PictureparkClient>();
}
```

Inject `IPictureparkClient` into your controller: 

```csharp
public class MyController : Controller
{
    private readonly IPictureparkClient _pictureparkClient;

    public MyController(IPictureparkClient pictureparkClient)
    {
        _pictureparkClient = pictureparkClient;
    }
    
    ...
```

### Usage with .NET 4.5.x framework

When installing the SDK in .NET 4.5.x targets you need to globally enable TLS 1.2: 

```csharp
ServicePointManager.SecurityProtocol = 
    SecurityProtocolType.Ssl3 | 
    SecurityProtocolType.Tls12 | 
    SecurityProtocolType.Tls11 | 
    SecurityProtocolType.Tls;
```
