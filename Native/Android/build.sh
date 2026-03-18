#!/bin/bash
# =============================================================================
# Build script per le librerie native Android (raylib + bridge)
#
# Prerequisiti:
#   - Android NDK installato (r25+ consigliato)
#   - CMake 3.20+
#   - Git (per scaricare raylib)
#
# Uso:
#   ./build.sh
#
# Oppure specificando il path dell'NDK:
#   ANDROID_NDK=/path/to/ndk ./build.sh
#
# Output:
#   Plants.Android/libs/arm64-v8a/libraylib.so
#   Plants.Android/libs/arm64-v8a/libplants_bridge.so
# =============================================================================

set -e

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
BUILD_DIR="$SCRIPT_DIR/build"

# Trova Android NDK
if [ -z "$ANDROID_NDK" ]; then
    # Prova path comuni
    if [ -d "$ANDROID_HOME/ndk-bundle" ]; then
        ANDROID_NDK="$ANDROID_HOME/ndk-bundle"
    elif [ -d "$ANDROID_SDK_ROOT/ndk-bundle" ]; then
        ANDROID_NDK="$ANDROID_SDK_ROOT/ndk-bundle"
    else
        # Cerca l'ultimo NDK installato
        NDK_DIR=$(find "${ANDROID_HOME:-$HOME/Android/Sdk}/ndk" -maxdepth 1 -type d 2>/dev/null | sort -V | tail -1)
        if [ -n "$NDK_DIR" ] && [ -d "$NDK_DIR" ]; then
            ANDROID_NDK="$NDK_DIR"
        fi
    fi
fi

if [ -z "$ANDROID_NDK" ] || [ ! -d "$ANDROID_NDK" ]; then
    echo "ERRORE: Android NDK non trovato."
    echo "Installa l'NDK e imposta ANDROID_NDK=/path/to/ndk"
    echo ""
    echo "Per installare:"
    echo "  sdkmanager --install 'ndk;25.2.9519653'"
    exit 1
fi

echo "=== Build librerie native Android ==="
echo "NDK: $ANDROID_NDK"
echo "Output: Plants.Android/libs/arm64-v8a/"
echo ""

# Crea directory di build
mkdir -p "$BUILD_DIR"

# Configura con CMake
echo ">>> Configurazione CMake..."
cmake -B "$BUILD_DIR" -S "$SCRIPT_DIR" \
    -DCMAKE_TOOLCHAIN_FILE="$ANDROID_NDK/build/cmake/android.toolchain.cmake" \
    -DANDROID_ABI=arm64-v8a \
    -DANDROID_PLATFORM=android-24 \
    -DANDROID_STL=c++_static \
    -DCMAKE_BUILD_TYPE=Release

# Compila
echo ">>> Compilazione..."
cmake --build "$BUILD_DIR" --config Release -j$(nproc 2>/dev/null || echo 4)

echo ""
echo "=== Build completata! ==="
echo "Librerie in: Plants.Android/libs/arm64-v8a/"
ls -la "$SCRIPT_DIR/../../Plants.Android/libs/arm64-v8a/"*.so 2>/dev/null || echo "(nessun file .so trovato)"
