/**
 * Plants Android Bridge
 *
 * Libreria nativa che gestisce:
 * - Inizializzazione EGL (contesto OpenGL ES 2.0)
 * - Wrapper per rlgl (layer GL di raylib)
 * - Input touch → mouse mapping
 * - Swap buffers
 *
 * Compilata insieme a raylib per Android (ARM64).
 * Il codice C# (Plants.Android) chiama queste funzioni via P/Invoke.
 */

#include <EGL/egl.h>
#include <GLES2/gl2.h>
#include <android/native_window.h>
#include <android/log.h>
#include <stdbool.h>
#include <string.h>

// raylib's rlgl - incluso dal sorgente raylib
#include "rlgl.h"

#define LOG_TAG "PlantsNative"
#define LOGI(...) __android_log_print(ANDROID_LOG_INFO, LOG_TAG, __VA_ARGS__)
#define LOGE(...) __android_log_print(ANDROID_LOG_ERROR, LOG_TAG, __VA_ARGS__)

// ========== Stato globale ==========

static EGLDisplay eglDisplay = EGL_NO_DISPLAY;
static EGLSurface eglSurface = EGL_NO_SURFACE;
static EGLContext eglContext = EGL_NO_CONTEXT;

static int screenWidth = 0;
static int screenHeight = 0;
static bool shouldClose = false;

// Touch state (mappato come mouse per il gioco)
static int touchX = 0;
static int touchY = 0;
static bool touchPressed = false;

// ========== EGL Setup ==========

bool InitDisplay(ANativeWindow* window, int width, int height)
{
    if (window == NULL)
    {
        LOGE("InitDisplay: window is NULL");
        return false;
    }

    screenWidth = width;
    screenHeight = height;

    // Ottieni il display EGL
    eglDisplay = eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (eglDisplay == EGL_NO_DISPLAY)
    {
        LOGE("eglGetDisplay failed");
        return false;
    }

    // Inizializza EGL
    EGLint major, minor;
    if (!eglInitialize(eglDisplay, &major, &minor))
    {
        LOGE("eglInitialize failed");
        return false;
    }

    LOGI("EGL initialized: %d.%d", major, minor);

    // Configura EGL per OpenGL ES 2.0
    const EGLint configAttribs[] = {
        EGL_SURFACE_TYPE, EGL_WINDOW_BIT,
        EGL_RED_SIZE, 8,
        EGL_GREEN_SIZE, 8,
        EGL_BLUE_SIZE, 8,
        EGL_ALPHA_SIZE, 8,
        EGL_DEPTH_SIZE, 16,
        EGL_RENDERABLE_TYPE, EGL_OPENGL_ES2_BIT,
        EGL_NONE
    };

    EGLConfig config;
    EGLint numConfigs;
    if (!eglChooseConfig(eglDisplay, configAttribs, &config, 1, &numConfigs) || numConfigs == 0)
    {
        LOGE("eglChooseConfig failed");
        return false;
    }

    // Imposta il formato del buffer nativo
    EGLint format;
    eglGetConfigAttrib(eglDisplay, config, EGL_NATIVE_VISUAL_ID, &format);
    ANativeWindow_setBuffersGeometry(window, 0, 0, format);

    // Crea la superficie EGL dalla finestra nativa
    eglSurface = eglCreateWindowSurface(eglDisplay, config, window, NULL);
    if (eglSurface == EGL_NO_SURFACE)
    {
        LOGE("eglCreateWindowSurface failed");
        return false;
    }

    // Crea il contesto OpenGL ES 2.0
    const EGLint contextAttribs[] = {
        EGL_CONTEXT_CLIENT_VERSION, 2,
        EGL_NONE
    };

    eglContext = eglCreateContext(eglDisplay, config, EGL_NO_CONTEXT, contextAttribs);
    if (eglContext == EGL_NO_CONTEXT)
    {
        LOGE("eglCreateContext failed");
        return false;
    }

    // Rendi il contesto corrente su questo thread
    if (!eglMakeCurrent(eglDisplay, eglSurface, eglSurface, eglContext))
    {
        LOGE("eglMakeCurrent failed");
        return false;
    }

    // Inizializza rlgl (il layer GL di raylib)
    rlglInit(width, height);

    // Setup viewport iniziale
    rlViewport(0, 0, width, height);
    rlMatrixMode(RL_PROJECTION);
    rlLoadIdentity();
    rlOrtho(0, width, height, 0, 0.0f, 1.0f);
    rlMatrixMode(RL_MODELVIEW);
    rlLoadIdentity();

    // Abilita blending per trasparenza
    rlEnableBackfaceCulling();

    shouldClose = false;

    LOGI("Display initialized: %dx%d", width, height);
    return true;
}

void SwapBuffers(void)
{
    // Flush rlgl batch prima dello swap
    rlDrawRenderBatchActive();

    // Swap dei buffer EGL
    if (eglDisplay != EGL_NO_DISPLAY && eglSurface != EGL_NO_SURFACE)
    {
        eglSwapBuffers(eglDisplay, eglSurface);
    }
}

void CleanupDisplay(void)
{
    LOGI("Cleaning up display");

    // Pulisci rlgl
    rlglClose();

    // Pulisci EGL
    if (eglDisplay != EGL_NO_DISPLAY)
    {
        eglMakeCurrent(eglDisplay, EGL_NO_SURFACE, EGL_NO_SURFACE, EGL_NO_CONTEXT);

        if (eglSurface != EGL_NO_SURFACE)
            eglDestroySurface(eglDisplay, eglSurface);

        if (eglContext != EGL_NO_CONTEXT)
            eglDestroyContext(eglDisplay, eglContext);

        eglTerminate(eglDisplay);
    }

    eglDisplay = EGL_NO_DISPLAY;
    eglSurface = EGL_NO_SURFACE;
    eglContext = EGL_NO_CONTEXT;
}

// ========== Screen ==========

void SetScreenSize(int width, int height)
{
    screenWidth = width;
    screenHeight = height;
    rlViewport(0, 0, width, height);
    LOGI("Screen size updated: %dx%d", width, height);
}

void RequestClose(void)
{
    shouldClose = true;
}

bool ShouldClose(void)
{
    return shouldClose;
}

// ========== Input ==========

void SetTouchPosition(int x, int y, bool pressed)
{
    touchX = x;
    touchY = y;
    touchPressed = pressed;
}

int GetTouchX(void)
{
    return touchX;
}

int GetTouchY(void)
{
    return touchY;
}

bool IsTouchPressed(void)
{
    return touchPressed;
}

// ========== rlgl Wrappers ==========

void RlglInit(int width, int height)
{
    rlglInit(width, height);
}

void ClearBuffers(void)
{
    rlClearScreenBuffers();
}

void FlushBatch(void)
{
    rlDrawRenderBatchActive();
}
