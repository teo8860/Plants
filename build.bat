dotnet publish -c Release -r win-x64 --self-contained true -p:OutputType=WinExe -p:PublishTrimmed=true -p:TrimMode=partial -o ./dist
xcopy /E /I /Y "Assets" "dist\Assets"
pause