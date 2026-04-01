cmake_minimum_required(VERSION 3.20)
project(plants_bridge C)

# ========== Raylib ==========
# Scarica raylib 5.0 source da GitHub e compilalo per Android
include(FetchContent)

FetchContent_Declare(
    raylib
    GIT_REPOSITORY https://github.com/raysan5/raylib.git
    GIT_TAG 5.0
    GIT_SHALLOW TRUE
)

# Configura raylib per Android come libreria STATICA
set(BUILD_EXAMPLES OFF CACHE BOOL "" FORCE)
set(BUILD_GAMES OFF CACHE BOOL "" FORCE)
set(PLATFORM "Android" CACHE STRING "" FORCE)
set(OPENGL_VERSION "ES 2.0" CACHE STRING "" FORCE)
set(BUILD_SHARED_LIBS OFF CACHE BOOL "" FORCE)

FetchContent_MakeAvailable(raylib)

# ========== Bridge Library ==========
# Unica .so che include raylib statico + il nostro bridge
add_library(plants_bridge SHARED bridge.c)

target_include_directories(plants_bridge PRIVATE
    ${raylib_SOURCE_DIR}/src
    ${raylib_SOURCE_DIR}/src/external
)

# Linka raylib staticamente dentro il bridge .so
# -Wl,--whole-archive per esportare TUTTI i simboli di raylib
target_link_libraries(plants_bridge
    -Wl,--whole-archive
    raylib
    -Wl,--no-whole-archive
    android
    log
    EGL
    GLESv2
    OpenSLES
    atomic
    m
)

# ========== Output ==========
set_target_properties(plants_bridge PROPERTIES
    LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_SOURCE_DIR}/../../Plants.Android/libs/arm64-v8a"
)
