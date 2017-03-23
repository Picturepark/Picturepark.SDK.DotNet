if (!(Test-Path "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/bin/Configuration.json")) { 
	(Get-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration_template.json") | 
	ForEach-Object { $_ -replace "{Server}", "$env:TestServer" } | 
	ForEach-Object { $_ -replace "{Username}", "$env:TestUsername" } | 
	ForEach-Object { $_ -replace "{Password}", "$env:TestPassword" } | 
	Set-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/bin/Configuration.json"
}

dotnet restore "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/" --no-cache
dotnet test "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Picturepark.SDK.V1.Tests.csproj" -c RELEASE