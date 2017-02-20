set /p apiKey=NuGet API Key: 
set /p version=Package Version: 

nuget.exe push Packages/Picturepark.SDK.V1.Contract.%version%.nupkg %apiKey% -s https://nuget.org
nuget.exe push Packages/Picturepark.SDK.V1.%version%.nupkg %apiKey% -s https://nuget.org
nuget.exe push Packages/Picturepark.SDK.V1.Localization.%version%.nupkg %apiKey% -s https://nuget.org