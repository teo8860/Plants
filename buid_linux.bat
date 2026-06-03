dotnet publish -c Release -r linux-x64 --self-contained true -p:OutputType=WinExe -o ./dist/linux
xcopy /E /I /Y "Assets" "dist\linux\Assets"
pause