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
var authClient = new AccessTokenAuthClient("https://api.mypcpserver.com", "AccessToken", "CustomerAlias");
using (var client = new PictureparkClient(authClient))
{
	var content = await client.Contents.GetAsync(contentId);
}
```

## Roadmap

- [ ] Chunked Upload support (Helper)
- [ ] Better Query (Filter) builder

