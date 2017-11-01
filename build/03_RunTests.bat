PowerShell -File "%~dp0/02_RunTests.ps1" || goto :error

goto :EOF
:error
echo Failed with error #%errorlevel%.
exit /b %errorlevel%