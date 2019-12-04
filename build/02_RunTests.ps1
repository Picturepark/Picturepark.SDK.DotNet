try {
	$AllProtocols = [System.Net.SecurityProtocolType]'Ssl3,Tls,Tls11,Tls12'
	[System.Net.ServicePointManager]::SecurityProtocol = $AllProtocols

	$customerInfo = Invoke-WebRequest "${Env:TestApiServer}/v1/info/customer" -Method Get -Headers @{ "Accept" = "application/json" } | ConvertFrom-Json
	
	${Env:TestCustomerId} = $customerInfo.CustomerId
	${Env:TestCustomerAlias} = $customerInfo.CustomerAlias

	$acr_values = "tenant:{""id"":""${Env:TestCustomerId}"",""alias"":""${Env:TestCustomerAlias}""}"


	$tokenParams = @{
		client_id     = ${Env:TestIdentityClientId};
		client_secret = ${Env:TestIdentitySecret};
		grant_type    = 'password';
		username      = ${Env:TestUsername};
		password      = ${Env:TestPassword};
		acr_values    = $acr_values;
		scope         = "openid profile picturepark_api all_scopes";
	}
	
	$result = Invoke-WebRequest ${Env:TestIdentityServer} -Method Post -Body $tokenParams | ConvertFrom-Json
	
	${Env:TestAccessToken} = $result.access_token
	
	if (!(Test-Path "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration.json")) { 
		(Get-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration_template.json") | 
			ForEach-Object { $_ -replace "{Server}", "$env:TestApiServer" } | 
			ForEach-Object { $_ -replace "{Username}", "$env:TestUsername" } | 
			ForEach-Object { $_ -replace "{Password}", "$env:TestPassword" } | 
			ForEach-Object { $_ -replace "{AccessToken}", "$env:TestAccessToken" } | 
			ForEach-Object { $_ -replace "{CustomerAlias}", "$env:TestCustomerAlias" } | 
			Set-Content "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Configuration.json"
	}
	
	dotnet restore "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/" --no-cache
	dotnet test "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Picturepark.SDK.V1.Tests.csproj" -c RELEASE -f netcoreapp2.0
    if ($lastexitcode -ne 0)
    {
        throw "A unit test failed."
    }

	dotnet test "$PSScriptRoot/../src/Picturepark.SDK.V1.Tests/Picturepark.SDK.V1.Tests.csproj" -c RELEASE -f net452
    
    if ($lastexitcode -ne 0)
    {
        throw "A unit test failed."
    }
} 
catch [Exception] { 
    "Failed to run unit tests: $_.Exception.Message" 
    $_.Exception.StackTrace 
    Exit 1 
}