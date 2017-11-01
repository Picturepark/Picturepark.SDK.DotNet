# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet.svg?label=build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet)
[![NuGet Version](https://img.shields.io/nuget/v/Picturepark.SDK.V1.svg)](https://www.nuget.org/packages?q=Picturepark)
[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet-y10cr.svg?label=CI+build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr)
[![MyGet CI](https://img.shields.io/myget/picturepark-sdk-dotnet-ci/vpre/Picturepark.SDK.V1.svg?label=CI+nuget)](https://www.myget.org/gallery/picturepark-sdk-dotnet-ci)

Links:
- [Picturepark Website](https://picturepark.com/)
- [SDK Documentation](docs/README.md)
- [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/index.html)

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

## SDK Development

Links: 

- [Build scripts](SCRIPTS.md)
- [Sources](src/)

### CI Builds

Branch: master

- NuGet CI Feed: https://www.myget.org/gallery/picturepark-sdk-dotnet-ci
- AppVeyor CI Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-y10cr

### Release Builds

Branch: release

- NuGet Feed: https://www.nuget.org/packages?q=Picturepark
- AppVeyor Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet
