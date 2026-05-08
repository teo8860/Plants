dotnet publish -c Release -r linux-x64 --self-contained true -p:OutputType=WinExe -o ./dist_linux
xcopy /E /I /Y "Assets" "dist_linux\Assets"
pause