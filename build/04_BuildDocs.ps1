$ErrorActionPreference = "Stop" 

cd $PSScriptRoot

# register package source (powershell nuget only supports v2 for now)
Register-PackageSource -Name nuget.org-v2-temp -Location https://www.nuget.org/api/v2 -ProviderName NuGet -ErrorAction SilentlyContinue

# fetch latest docfx.console package
Find-Package -Name docfx.console -Source nuget.org-v2-temp | Save-Package -Path .

# unzip package
Move-Item *.nupkg docfx.zip
Expand-Archive docfx.zip -DestinationPath temp

./temp/tools/docfx.exe ../docs/sdk/docfx.json

Remove-Item -Force -Recurse temp
Remove-Item -Force *.zip

# unregister package source
Unregister-PackageSource -Name nuget.org-v2-temp