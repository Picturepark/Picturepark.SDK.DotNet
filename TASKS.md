# Picturepark Dotnet SDK

A .NET SDK for integrating with the Picturepark API v1.

## Support Platforms
 - .NET Standard 1.3

## Security

Picturepark uses the following ssl / cipher configuration.
https://mozilla.github.io/server-side-tls/ssl-config-generator/?server=haproxy-1.5.14&openssl=1.0.1e&hsts=yes&profile=modern

## Setup

To get started with Picturepark.SDK, we recommend you add it to your project using NuGet.

To install `Picturepark.SDK`, run the following command in the Package Manager Console:

```PM> Install-Package Picturepark.SDK.V1```

## Example

```
using(var authClient = new UsernamePasswordAuthClient(basePath, username, password))
using(var tokenRefresher = new AccessTokenRefresher(authClient)) 
using (var client = new PictureparkClient(tokenRefresher))
{
	var asset = await client.Assets.GetAsync(assetId);
}
```

## Roadmap

- [x] SchemaHelper.CreateOrUpdate()
- [x] .Net Core compatibility
- [x] Remove Reactive Extensions dependency
- [x] Local NuGet Config (VIT Package Server)
- [x] Range Headers for Downloads (Swagger)
- [x] Create packages Picturepark.SDK.V1.Contract & Picturepark.SDK.V1
- [x] Refactoring: Inject AuthClient into generated clients
- [x] Completly wrap generated endpoints
- [x] Remove DotLiquid and GetText dependency (create seperate Package)
- [x] Enable Stylecop for .NET Core Projects (http://stackoverflow.com/questions/37482483/stylecop-analyzers-in-aspnetcore-application-and-own-rules-set)
- [ ] Configurable HTTP timeout (currently set to 1 minute)
- [x] Use StringEnumConverter (needs backend change in Nancy)
- [x] Fix CI (run x-unit tests, use dotnet pack to publish to NuGet)
- [ ] Chunked Upload support (Helper)
- [ ] Better Query (Filter) builder

## NSwag extensions
- [x] Use Streams for Downloads (NSwag)
- [x] [JsonObject(MemberSerialization.OptIn)] for Dictionary<string, string> (NSwag)

