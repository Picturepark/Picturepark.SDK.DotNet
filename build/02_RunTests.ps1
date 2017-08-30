$tokenParams = @{
  client_id=${Env:TestIdentityClientId};
  client_secret=${Env:TestIdentitySecret};
  grant_type='password';
  username=${Env:TestUsername};
  password=${Env:TestPassword};
}

$result = Invoke-WebRequest ${Env:TestIdentityServer} -Method Post -Body $tokenParams | ConvertFrom-Json

${Env:TestAccessToken} = $result.access_token

if (!(Test-Path "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration.json")) { 
	(Get-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration_template.json") | 
	ForEach-Object { $_ -replace "{Server}", "$env:TestServer" } | 
	ForEach-Object { $_ -replace "{Username}", "$env:TestUsername" } | 
	ForEach-Object { $_ -replace "{Password}", "$env:TestPassword" } | 
	ForEach-Object { $_ -replace "{AccessToken}", "$env:TestAccessToken" } | 
	Set-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration.json"
}

dotnet restore "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/" --no-cache
dotnet test "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Picturepark.SDK.V1.Tests.csproj" -c RELEASE