# Picturepark Content Platform .NET SDK
## Picturepark.Sdk.DotNet

[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet.svg?label=build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet)
[![NuGet Version](https://img.shields.io/nuget/v/Picturepark.SDK.V1.svg)](https://www.nuget.org/packages?q=Picturepark)
[![Build status](https://img.shields.io/appveyor/ci/Picturepark/picturepark-sdk-dotnet-7lqi5/master.svg?label=CI+build)](https://ci.appveyor.com/project/Picturepark/picturepark-sdk-dotnet-7lqi5)
[![MyGet CI](https://img.shields.io/myget/picturepark-sdk-dotnet-ci/vpre/Picturepark.SDK.V1.svg?label=CI+nuget)](https://www.myget.org/gallery/picturepark-sdk-dotnet-ci)

Links:
- [Picturepark Website](https://picturepark.com/)
- [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/index.html)
- [API Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/api/index.html)
- [Getting started](docs/README.md)

## NuGet Packages

**Public APIs**:

- **[Picturepark.SDK.V1](https://www.nuget.org/packages/Picturepark.SDK.V1) (.NET Standard 1.3+ & .NET 4.5+):** 
    - Client implementations to access the Picturepark server
    - [Getting started](docs/README.md)
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.html)
- **[Picturepark.SDK.V1.Contract](https://www.nuget.org/packages/Picturepark.SDK.V1.Contract) (.NET Standard 1.3+ & .NET 4.5+):** 
    - DTO classes and client interfaces 
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.Contract.html)
- **[Picturepark.SDK.V1.Localization](https://www.nuget.org/packages/Picturepark.SDK.V1.Localization) (.NET Standard 1.3+ & .NET 4.5+):** 
    - Utilities to translate server messages
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.Localization.html)

**Management APIs**:

- **[Picturepark.SDK.V1.CloudManager](https://www.nuget.org/packages/Picturepark.SDK.V1.CloudManager) (.NET Standard 1.3+ & .NET 4.5+)**
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.CloudManager.html)
- **[Picturepark.SDK.V1.ServiceProvider](https://www.nuget.org/packages/Picturepark.SDK.V1.ServiceProvider) (.NET Standard 2.0+ & .NET 4.6.1+)**
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.ServiceProvider.html)

## Compatibility matrix

| SDK version | Picturepark CP version |
| ----------- | ---------------------- |
| `1.0.x`     | `10.0.x`               |
| `1.1.x`     | `10.1.x`               |
| `1.2.x`     | `10.2.x`               |
| `1.3.x`     | `10.3.x`               |
| `1.4.x`     | `10.4.x`               |
| `1.5.x`     | `10.5.x`               |
| `1.6.x`     | `10.6.x`               |
| `1.7.x`     | `10.7.x`               |

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