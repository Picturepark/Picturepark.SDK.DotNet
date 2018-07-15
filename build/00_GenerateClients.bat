pushd "%~dp0/../"
cmd /c npm update
cmd /c npm run nswag
popd