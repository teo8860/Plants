dotnet publish -c Release -r win-x64 --self-contained true -p:OutputType=WinExe -o ./dist
xcopy /E /I /Y "Assets" "dist\Assets"
