@echo off
REM =============================================================================
REM Build librerie native Android (raylib + bridge) - Windows
REM
REM Prerequisiti:
REM   - Android NDK installato (r25+ consigliato)
REM   - CMake 3.20+ (nel PATH)
REM   - Git (per scaricare raylib)
REM
REM Uso:
REM   build.bat
REM
REM Oppure specificando il path dell'NDK:
REM   set ANDROID_NDK=C:\path\to\ndk
REM   build.bat
REM
REM Output:
REM   Plants.Android\libs\arm64-v8a\libraylib.so
REM   Plants.Android\libs\arm64-v8a\libplants_bridge.so
REM =============================================================================

setlocal

set SCRIPT_DIR=%~dp0
set BUILD_DIR=%SCRIPT_DIR%build

REM Trova Android NDK
if not defined ANDROID_NDK (
    if defined ANDROID_HOME (
        REM Cerca NDK installato tramite SDK Manager
        for /f "delims=" %%i in ('dir /b /ad /o-n "%ANDROID_HOME%\ndk" 2^>nul') do (
            set ANDROID_NDK=%ANDROID_HOME%\ndk\%%i
            goto :found_ndk
        )
    )
    if defined LOCALAPPDATA (
        for /f "delims=" %%i in ('dir /b /ad /o-n "%LOCALAPPDATA%\Android\Sdk\ndk" 2^>nul') do (
            set ANDROID_NDK=%LOCALAPPDATA%\Android\Sdk\ndk\%%i
            goto :found_ndk
        )
    )
    echo ERRORE: Android NDK non trovato.
    echo Installa l'NDK e imposta ANDROID_NDK=C:\path\to\ndk
    echo.
    echo Per installare tramite Android Studio:
    echo   SDK Manager ^> SDK Tools ^> NDK (Side by side)
    exit /b 1
)

:found_ndk
echo === Build librerie native Android ===
echo NDK: %ANDROID_NDK%
echo Output: Plants.Android\libs\arm64-v8a\
echo.

REM Crea directory
if not exist "%BUILD_DIR%" mkdir "%BUILD_DIR%"

REM Configura CMake
echo ^>^>^> Configurazione CMake...
cmake -B "%BUILD_DIR%" -S "%SCRIPT_DIR%" ^
    -G "Ninja" ^
    -DCMAKE_TOOLCHAIN_FILE="%ANDROID_NDK%\build\cmake\android.toolchain.cmake" ^
    -DANDROID_ABI=arm64-v8a ^
    -DANDROID_PLATFORM=android-24 ^
    -DANDROID_STL=c++_static ^
    -DCMAKE_BUILD_TYPE=Release ^
    -DCMAKE_MAKE_PROGRAM="%ANDROID_NDK%\prebuilt\windows-x86_64\bin\ninja.exe"

if errorlevel 1 (
    echo ERRORE nella configurazione CMake
    exit /b 1
)

REM Compila
echo ^>^>^> Compilazione...
cmake --build "%BUILD_DIR%" --config Release

if errorlevel 1 (
    echo ERRORE nella compilazione
    exit /b 1
)

echo.
echo === Build completata! ===
echo Librerie in: Plants.Android\libs\arm64-v8a\
dir "%SCRIPT_DIR%..\..\Plants.Android\libs\arm64-v8a\*.so" 2>nul

endlocal
