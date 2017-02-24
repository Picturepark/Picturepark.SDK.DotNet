# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet.svg?label=build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet)
[![NuGet Version](https://img.shields.io/nuget/v/Picturepark.SDK.V1.svg)](https://www.nuget.org/packages?q=Picturepark)
[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet-y10cr.svg?label=CI+build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr)
[![MyGet CI](https://img.shields.io/myget/picturepark-sdk-dotnet-ci/vpre/Picturepark.SDK.V1.svg)](https://www.myget.org/gallery/picturepark-sdk-dotnet-ci)

Links: 
- [Picturepark Website](https://picturepark.com/)
- [API Documentation](docs/README.md)

## Usage

Install required NuGet package: 

    Install-Package Picturepark.SDK.V1
    
Create new `PictureparkClient` and access remote PCP server: 

```csharp
var authClient = new UsernamePasswordAuthClient("http://mypcpserver.com", username, password); 
using (var client = new PictureparkClient(authClient))
{
    var asset = await client.Assets.GetAsync("myAssetId");
}
```

### ASP.NET Core

Register the Picturepark .NET service classes in the ASP.NET Core dependency injection system (`Startup.cs`): 

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddApplicationInsightsTelemetry(Configuration);
	services.AddMvc();

	services.AddSingleton<IAuthClient>(new UsernamePasswordAuthClient("myUrl", "myUsername", "myPassword"));
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

### Target .NET 4.5.x

When installing the SDK in .NET 4.5.x targets you need to globally enable TLS 1.2: 

```csharp
ServicePointManager.SecurityProtocol = 
    SecurityProtocolType.Ssl3 | 
    SecurityProtocolType.Tls12 | 
    SecurityProtocolType.Tls11 | 
    SecurityProtocolType.Tls;
```

## NuGet Packages

All packages support the following target frameworks: 

- .NET 4.5+
- .NET Standard 1.3+

**Public APIs**: 

- [Picturepark.SDK.V1](https://www.nuget.org/packages/Picturepark.SDK.V1): Client implementations to access the Picturepark server
- [Picturepark.SDK.V1.Contract](https://www.nuget.org/packages/Picturepark.SDK.V1.Contract): DTO classes and client interfaces 
- [Picturepark.SDK.V1.Localization](https://www.nuget.org/packages/Picturepark.SDK.V1.Localization): Utilities to translate server messages

**Management APIs**: 

- Picturepark.SDK.V1.CloudManager
- Picturepark.SDK.V1.ServiceProvider

## Development

Links: 

- [Build Scripts](build/README.md)
- [Sources](src/)

### CI Builds

Branch: master

- NuGet CI Feed: https://www.myget.org/gallery/picturepark-sdk-dotnet-ci
- AppVeyor CI Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr

### Release Builds

Branch: release

- NuGet Feed: https://www.nuget.org/packages?q=Picturepark
- AppVeyor Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet


