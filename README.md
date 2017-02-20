# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

Links: 

- [Documentation](docs/README.md)
- [Sources](src/)

## Usage

Install required NuGet package: 

    Install-Package Picturepark.SDK.V1
    
Create new `PictureparkClient` and access remote PCP server: 

```cs
using (var authClient = new UsernamePasswordAuthClient("http://mypcpserver.com", username, password))
using (var client = new PictureparkClient(authClient))
{
    var asset = await client.Assets.GetAsync("myAssetId");
}
```

## Development

- [Build Scripts](build/README.md)

NuGet Feed: https://www.nuget.org/packages?q=Picturepark

MyGet CI Feed: https://www.myget.org/feed/Packages/picturepark-sdk-dotnet-ci

AppVeyor Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet

AppVeyor CI Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr

