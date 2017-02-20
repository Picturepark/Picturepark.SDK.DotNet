# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet.svg?label=build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet)
[![MyGet CI](https://img.shields.io/myget/picturepark-sdk-dotnet-ci/vpre/Picturepark.SDK.V1.svg)]()
[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet-y10cr.svg?label=CI+build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr)

Links: 

- [Documentation](docs/README.md)
- [Build Scripts](build/README.md)
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

NuGet Feed: https://www.nuget.org/packages?q=Picturepark

MyGet CI Feed: https://www.myget.org/feed/Packages/picturepark-sdk-dotnet-ci

AppVeyor Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet

AppVeyor CI Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr

