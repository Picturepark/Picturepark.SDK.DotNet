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

- **[Picturepark.SDK.V1](https://www.nuget.org/packages/Picturepark.SDK.V1) (.NET Standard 2.0):** 
    - Client implementations to access the Picturepark server
    - [Getting started](docs/README.md)
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.html)
- **[Picturepark.SDK.V1.Contract](https://www.nuget.org/packages/Picturepark.SDK.V1.Contract) (.NET Standard 2.0):** 
    - DTO classes and client interfaces 
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.Contract.html)
- **[Picturepark.SDK.V1.Localization](https://www.nuget.org/packages/Picturepark.SDK.V1.Localization) (.NET Standard 2.0):** 
    - Utilities to translate server messages
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.Localization.html)

**Management APIs**:

- **[Picturepark.SDK.V1.CloudManager](https://www.nuget.org/packages/Picturepark.SDK.V1.CloudManager) (.NET Standard 2.0)**
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.CloudManager.html)
- **[Picturepark.SDK.V1.ServiceProvider](https://www.nuget.org/packages/Picturepark.SDK.V1.ServiceProvider) (.NET Standard 2.0)**
    - [SDK Documentation](https://picturepark.github.io/Picturepark.SDK.DotNet/sdk/site/api/Picturepark.SDK.V1.ServiceProvider.html)

## Compatibility matrix

| SDK version | Picturepark CP version |
|-------------|------------------------|
| `11.0.x`    | `11.0.x`               |
| `11.1.x`    | `11.1.x`               |
| `11.2.x`    | `11.2.x`               |
| `11.3.x`    | `11.3.x`               |
| `11.4.x`    | `11.4.x`               |
| `11.5.x`    | `11.5.x`               |
| `11.6.x`    | `11.6.x`               |
| `11.7.x`    | `11.7.x`               |
| `11.8.x`    | `11.8.x`               |

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
