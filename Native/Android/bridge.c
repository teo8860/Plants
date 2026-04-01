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
#include <android/asset_manager.h>
#include <android/asset_manager_jni.h>
#include <android/log.h>
#include <stdbool.h>
#include <string.h>

// raylib headers
#include "rlgl.h"
#include "utils.h"

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

// ========== Asset Manager ==========
#include <jni.h>

// Riceve il jobject AssetManager da Java/C# e lo converte in AAssetManager* nativo
void Bridge_InitAssetManagerJni(JNIEnv *env, jobject javaAssetManager, const char *internalDataPath)
{
    AAssetManager *manager = AAssetManager_fromJava(env, javaAssetManager);
    LOGI("InitAssetManager: javaObj=%p, native=%p, dataPath=%s", javaAssetManager, manager, internalDataPath ? internalDataPath : "(null)");
    if (manager != NULL)
    {
        InitAssetManager(manager, internalDataPath);
    }
    else
    {
        LOGE("AAssetManager_fromJava returned NULL!");
    }
}

// ========== EGL Setup ==========

bool Bridge_InitDisplay(ANativeWindow* window, int width, int height)
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

void Bridge_SwapBuffers(void)
{
    // Flush rlgl batch prima dello swap
    rlDrawRenderBatchActive();

    // Swap dei buffer EGL
    if (eglDisplay != EGL_NO_DISPLAY && eglSurface != EGL_NO_SURFACE)
    {
        eglSwapBuffers(eglDisplay, eglSurface);
    }
}

void Bridge_CleanupDisplay(void)
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

void Bridge_SetScreenSize(int width, int height)
{
    screenWidth = width;
    screenHeight = height;
    rlViewport(0, 0, width, height);
    LOGI("Screen size updated: %dx%d", width, height);
}

void Bridge_RequestClose(void)
{
    shouldClose = true;
}

bool Bridge_ShouldClose(void)
{
    return shouldClose;
}

// ========== Input ==========

void Bridge_SetTouchPosition(int x, int y, bool pressed)
{
    touchX = x;
    touchY = y;
    touchPressed = pressed;
}

int Bridge_GetTouchX(void)
{
    return touchX;
}

int Bridge_GetTouchY(void)
{
    return touchY;
}

bool Bridge_IsTouchPressed(void)
{
    return touchPressed;
}

// ========== rlgl Wrappers ==========

void Bridge_RlglInit(int width, int height)
{
    rlglInit(width, height);
}

void Bridge_ClearBuffers(void)
{
    rlClearScreenBuffers();
}

void Bridge_FlushBatch(void)
{
    rlDrawRenderBatchActive();
}

// Test: solo clear senza swap
void Bridge_TestFrame_ClearOnly(void)
{
    glClearColor(0.2f, 0.3f, 0.2f, 1.0f);  // Verde scuro
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);
}

// Test: clear con colore e swap direttamente con GL, bypassando rlgl
void Bridge_TestFrame(void)
{
    glClearColor(0.0f, 0.5f, 1.0f, 1.0f);  // Azzurro
    glClear(GL_COLOR_BUFFER_BIT | GL_DEPTH_BUFFER_BIT);

    if (eglDisplay != EGL_NO_DISPLAY && eglSurface != EGL_NO_SURFACE)
    {
        EGLBoolean result = eglSwapBuffers(eglDisplay, eglSurface);
        if (!result)
        {
            LOGE("Bridge_TestFrame: eglSwapBuffers FAILED, error=0x%x", eglGetError());
        }
    }
    else
    {
        LOGE("Bridge_TestFrame: no display/surface!");
    }
}

// ========== FBO Diagnostics ==========

int Bridge_GetDefaultFramebuffer(void)
{
    GLint fbo = 0;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &fbo);
    return fbo;
}

int Bridge_CheckFramebufferStatus(unsigned int fboId)
{
    glBindFramebuffer(GL_FRAMEBUFFER, fboId);
    GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    glBindFramebuffer(GL_FRAMEBUFFER, 0);
    LOGI("FBO %u status: 0x%x (complete=0x%x)", fboId, status, GL_FRAMEBUFFER_COMPLETE);
    return (int)status;
}

// Test: crea un FBO nativo, renderizza rosso, disegnalo sullo schermo
void Bridge_TestFBO(int screenW, int screenH)
{
    // Salva il framebuffer corrente
    GLint defaultFBO = 0;
    glGetIntegerv(GL_FRAMEBUFFER_BINDING, &defaultFBO);
    LOGI("TestFBO: default FBO = %d", defaultFBO);

    // Crea texture
    GLuint texId;
    glGenTextures(1, &texId);
    glBindTexture(GL_TEXTURE_2D, texId);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA, 64, 64, 0, GL_RGBA, GL_UNSIGNED_BYTE, NULL);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);

    // Crea FBO
    GLuint fboId;
    glGenFramebuffers(1, &fboId);
    glBindFramebuffer(GL_FRAMEBUFFER, fboId);
    glFramebufferTexture2D(GL_FRAMEBUFFER, GL_COLOR_ATTACHMENT0, GL_TEXTURE_2D, texId, 0);

    GLenum status = glCheckFramebufferStatus(GL_FRAMEBUFFER);
    LOGI("TestFBO: FBO %u status=0x%x (complete=0x%x)", fboId, status, GL_FRAMEBUFFER_COMPLETE);

    if (status == GL_FRAMEBUFFER_COMPLETE)
    {
        // Render rosso nel FBO
        glViewport(0, 0, 64, 64);
        glClearColor(1.0f, 0.0f, 0.0f, 1.0f);
        glClear(GL_COLOR_BUFFER_BIT);
    }

    // Torna al default framebuffer
    glBindFramebuffer(GL_FRAMEBUFFER, defaultFBO);
    glViewport(0, 0, screenW, screenH);

    // Disegna la texture FBO sullo schermo usando rlgl
    rlDrawRenderBatchActive();

    // Setup ortho per schermo
    rlMatrixMode(RL_PROJECTION);
    rlLoadIdentity();
    rlOrtho(0, screenW, screenH, 0, 0.0, 1.0);
    rlMatrixMode(RL_MODELVIEW);
    rlLoadIdentity();

    // Disegna la texture manualmente via rlgl
    rlSetTexture(texId);
    rlBegin(RL_QUADS);
        rlColor4ub(255, 255, 255, 255);
        rlTexCoord2f(0, 0); rlVertex2f(100, 100);
        rlTexCoord2f(0, 1); rlVertex2f(100, 400);
        rlTexCoord2f(1, 1); rlVertex2f(400, 400);
        rlTexCoord2f(1, 0); rlVertex2f(400, 100);
    rlEnd();
    rlSetTexture(0);

    rlDrawRenderBatchActive();

    LOGI("TestFBO: disegnato FBO tex %u sullo schermo", texId);
}

// ========== Touch → Raylib Input ==========

// Defined in rcore_android.c (custom additions)
extern void SetTouchState(int id, bool pressed);
extern void SetTouchPositionXY(int id, float x, float y);
extern void CycleInputState(void);

void Bridge_SetTouchInput(int x, int y, bool pressed)
{
    // Feed touch into raylib's CORE.Input.Touch + Mouse
    SetTouchState(0, pressed);
    SetTouchPositionXY(0, (float)x, (float)y);
}

void Bridge_PollInputEvents(void)
{
    // Cycle previous = current state (must be called at START of each frame,
    // BEFORE setting new touch state)
    CycleInputState();
}

// Dummy main - richiesto da rcore.c Android (android_main -> main)
// Non viene mai chiamato perché gestiamo EGL direttamente dal bridge
int main(int argc, char *argv[])
{
    (void)argc;
    (void)argv;
    return 0;
}
