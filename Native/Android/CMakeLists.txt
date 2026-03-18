cmake_minimum_required(VERSION 3.20)
project(plants_bridge C)

# ========== Raylib ==========
# Scarica raylib 4.0 source da GitHub e compilalo per Android
include(FetchContent)

FetchContent_Declare(
    raylib
    GIT_REPOSITORY https://github.com/raysan5/raylib.git
    GIT_TAG 4.0.0
    GIT_SHALLOW TRUE
)

# Configura raylib per Android (solo rlgl + textures, no window management)
set(BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(BUILD_GAMES OFF CACHE BOOL "" FORCE)
set(PLATFORM "Desktop" CACHE STRING "" FORCE)  # Usa Desktop mode per avere rlgl senza NativeActivity
set(OPENGL_VERSION "ES 2.0" CACHE STRING "" FORCE)
set(BUILD_SHARED_LIBS ON CACHE BOOL "" FORCE)

FetchContent_MakeAvailable(raylib)

# ========== Bridge Library ==========
add_library(plants_bridge SHARED bridge.c)

target_include_directories(plants_bridge PRIVATE
    ${raylib_SOURCE_DIR}/src      # raylib headers (rlgl.h, raylib.h)
    ${raylib_SOURCE_DIR}/src/external  # glad, etc.
)

target_link_libraries(plants_bridge
    raylib
    android
    log
    EGL
    GLESv2
)

# ========== Output ==========
# Le .so verranno copiate in Plants.Android/libs/arm64-v8a/
set_target_properties(plants_bridge PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../../Plants.Android/libs/arm64-v8a"
)

set_target_properties(raylib PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../../Plants.Android/libs/arm64-v8a"
)
