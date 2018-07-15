rmdir "%~dp0/Packages" /Q /S nonemptydir
mkdir "%~dp0/Packages"

del "%~dp0/../src/Picturepark.SDK.V1.Contract/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1.Contract/" --no-cache
dotnet pack "%~dp0/../src/Picturepark.SDK.V1.Contract/" --output "%~dp0/Packages" --configuration Release

del "%~dp0/../src/Picturepark.SDK.V1/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1/" --no-cache
dotnet pack "%~dp0/../src/Picturepark.SDK.V1/" --output "%~dp0/Packages" --configuration Release

del "%~dp0/../src/Picturepark.SDK.V1.Localization/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1.Localization/" --no-cache
dotnet pack "%~dp0/../src/Picturepark.SDK.V1.Localization/" --output "%~dp0/Packages" --configuration Release

del "%~dp0/../src/Picturepark.SDK.V1.ServiceProvider/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1.ServiceProvider/" --no-cache
dotnet pack "%~dp0/../src/Picturepark.SDK.V1.ServiceProvider/" --output "%~dp0/Packages" --configuration Release

del "%~dp0/../src/Picturepark.SDK.V1.CloudManager/project.lock.json"
dotnet restore "%~dp0/../src/Picturepark.SDK.V1.CloudManager/" --no-cache
dotnet pack "%~dp0/../src/Picturepark.SDK.V1.CloudManager/" --output "%~dp0/Packages" --configuration Release