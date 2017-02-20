rmdir Packages /Q /S nonemptydir
mkdir Packages

cd "../src/Picturepark.SDK.V1.Contract"

del project.lock.json
dotnet restore --no-cache
dotnet pack --output "../../build/Packages" --configuration Release

cd "../Picturepark.SDK.V1"

del project.lock.json
dotnet restore --no-cache
dotnet pack --output "../../build/Packages" --configuration Release

cd "../Picturepark.SDK.V1.Localization"

del project.lock.json
dotnet restore --no-cache
dotnet pack --output "../../build/Packages" --configuration Release

cd "../Picturepark.SDK.V1.ServiceProvider"

del project.lock.json
dotnet restore --no-cache
dotnet pack --output "../../build/Packages" --configuration Release

cd "../Picturepark.SDK.V1.CloudManager"

del project.lock.json
dotnet restore --no-cache
dotnet pack --output "../../build/Packages" --configuration Release

cd "../Picturepark.SDK.V1.Tests"

del project.lock.json
dotnet restore --no-cache
dotnet build --configuration Release

cd "../../build"