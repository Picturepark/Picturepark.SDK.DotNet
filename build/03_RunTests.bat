del "%~dp0/../src/Picturepark.SDK.V1.Tests/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1.Tests/" --no-cache
dotnet test "%~dp0/../src/Picturepark.SDK.V1.Tests" -c RELEASE