:: Build per sviluppo (Debug, con console visibile per log)
dotnet build -c Debug -r win-x64
if %ERRORLEVEL% NEQ 0 (
    echo Build fallito!
    pause
    exit /b 1
)
echo Build completato.
pause
