# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet.svg?label=build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet)
[![NuGet Version](https://img.shields.io/nuget/v/Picturepark.SDK.V1.svg)](https://www.nuget.org/packages?q=Picturepark)
[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet-y10cr.svg?label=CI+build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-7lqi5)
[![MyGet CI](https://img.shields.io/myget/picturepark-sdk-dotnet-ci/vpre/Picturepark.SDK.V1.svg?label=CI+nuget)](https://www.myget.org/gallery/picturepark-sdk-dotnet-ci)

Links:
- [Picturepark Website](https://picturepark.com/)
- [SDK Documentation](docs/README.md)
- [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/index.html)

## NuGet Packages

**Public APIs**:

- **[Picturepark.SDK.V1](https://www.nuget.org/packages/Picturepark.SDK.V1) (.NET Standard 1.3+ & .NET 4.5+):** 
    - Client implementations to access the Picturepark server
    - [SDK Documentation](docs/README.md)
    - [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/api/Picturepark.SDK.V1.html)
- **[Picturepark.SDK.V1.Contract](https://www.nuget.org/packages/Picturepark.SDK.V1.Contract) (.NET Standard 1.3+ & .NET 4.5+):** 
    - DTO classes and client interfaces 
    - [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/api/Picturepark.SDK.V1.Contract.html)
- **[Picturepark.SDK.V1.Localization](https://www.nuget.org/packages/Picturepark.SDK.V1.Localization) (.NET Standard 1.3+ & .NET 4.5+):** 
    - Utilities to translate server messages
    - [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/api/Picturepark.SDK.V1.Localization.html)

**Management APIs**:

- **[Picturepark.SDK.V1.CloudManager](https://www.nuget.org/packages/Picturepark.SDK.V1.CloudManager) (.NET Standard 1.3+ & .NET 4.5+)**
    - [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/api/Picturepark.SDK.V1.CloudManager.html)
- **[Picturepark.SDK.V1.ServiceProvider](https://www.nuget.org/packages/Picturepark.SDK.V1.ServiceProvider) (.NET Standard 1.6+ & .NET 4.6+)**
    - [API Documentation](https://rawgit.com/Picturepark/Picturepark.SDK.DotNet/master/docs/api/site/api/Picturepark.SDK.V1.ServiceProvider.html)

## SDK Development

Links: 

- [Build scripts](SCRIPTS.md)
- [Sources](src/)

### Client generation

Run the following commands to regenerate the clients based on the Swagger specifications in `/swagger`: 

    npm install
	npm run nswag

### CI Builds

Branch: master

- NuGet CI Feed: https://www.myget.org/gallery/picturepark-sdk-dotnet-ci
- AppVeyor CI Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-7lqi5

### Release Builds

Branch: release

- NuGet Feed: https://www.nuget.org/packages?q=Picturepark
- AppVeyor Build: https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet