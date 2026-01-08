using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Numerics;

namespace Plants;

public class WeatherRender : GameElement
{
    private CircularBuffer rainBuffer;
    private CircularBuffer snowBuffer;
    private int screenWidth;
    private int screenHeight;
    private float lightningTimer = 0f;
    private bool showLightning = false;
    private float[] cloudOffsets = new float[3];

    public WeatherRender()
    {
        screenHeight = GameProperties.cameraHeight + (int)Rendering.camera.position.Y;
        screenWidth = GameProperties.cameraWidth;
        rainBuffer = new CircularBuffer();
        snowBuffer = new CircularBuffer();

        for (int i = 0; i < cloudOffsets.Length; i++)
        {
            cloudOffsets[i] = RandomHelper.Int(0, 300);
        }
    }

    private ParticleData AddToBuffer(CircularBuffer buffer)
    {
        if ((buffer.head + 1) % buffer.MAX_PARTICLES != buffer.tail)
        {
            ParticleData particle = buffer.buffer[buffer.head];
            buffer.head = (buffer.head + 1) % buffer.MAX_PARTICLES;
            return particle;
        }
        return null;
    }

    public override void Draw()
    {
        switch (WeatherManager.GetCurrentWeather())
        {
            case Weather.Cloudy:
                DrawClouds();
                break;
            case Weather.Rainy:
                UpdateRain(false);
                DrawRain();
                break;
            case Weather.Stormy:
                UpdateRain(true);
                DrawRain();
                UpdateLightning();
                DrawLightning();
                break;
            case Weather.Foggy:
                DrawFog();
                break;
            case Weather.Snowy:
                UpdateSnow();
                DrawSnow();
                break;
        }
    }



    private void UpdateRain(bool heavy)
    {
        int particlesToSpawn = heavy ? 8 : 4;

        for (int I = 0; I < particlesToSpawn; I++)
        {
            ParticleData newParticle = AddToBuffer(rainBuffer);
            if (newParticle != null)
            {
                newParticle.position = new Vector2(RandomHelper.Int(0, screenWidth), -10);
                newParticle.alive = true;
                newParticle.radius = 2.0f;
                newParticle.color = new Color(173, 216, 230, heavy ? (byte)180 : (byte)120);
                newParticle.velocity = new Vector2(0, heavy ? 12 : 8);
            }
        }

        int i = rainBuffer.tail;
        while (i != rainBuffer.head)
        {
            ParticleData p = rainBuffer.buffer[i];
            if (p.alive)
            {
                p.position += p.velocity;

                if (p.position.Y > screenHeight || p.position.X > screenWidth)
                {
                    p.alive = false;
                }
            }
            i = (i + 1) % rainBuffer.MAX_PARTICLES;
        }

        while (rainBuffer.tail != rainBuffer.head && !rainBuffer.buffer[rainBuffer.tail].alive)
        {
            rainBuffer.tail = (rainBuffer.tail + 1) % rainBuffer.MAX_PARTICLES;
        }
    }

    private void DrawRain()
    {
        int i = rainBuffer.tail;
        while (i != rainBuffer.head)
        {
            ParticleData p = rainBuffer.buffer[i];
            if (p.alive)
            {
                Graphics.DrawLine(
                    (int)p.position.X,
                    (int)p.position.Y,
                    (int)(p.position.X - 2),
                    (int)(p.position.Y - 15),
                    p.color
                );
            }
            i = (i + 1) % rainBuffer.MAX_PARTICLES;
        }
    }

    private void UpdateSnow()
    {
        for (int I = 0; I < 3; I++)
        {
            ParticleData newParticle = AddToBuffer(snowBuffer);
            if (newParticle != null)
            {
                newParticle.position = new Vector2(RandomHelper.Int(0, screenWidth), -10);
                newParticle.alive = true;
                newParticle.radius = 3.0f;
                newParticle.color = new Color(255, 255, 255, 200);
                newParticle.velocity = new Vector2(0, 1 + RandomHelper.Int(2));
                newParticle.lifetime = 0;
            }
        }

        int i = snowBuffer.tail;
        while (i != snowBuffer.head)
        {
            ParticleData p = snowBuffer.buffer[i];
            if (p.alive)
            {
                p.lifetime += 0.016f;
                p.position.X += MathF.Sin(p.lifetime * 2) * 0.5f;
                p.position.Y += p.velocity.Y;

                if (p.position.Y > screenHeight)
                {
                    p.alive = false;
                }
            }
            i = (i + 1) % snowBuffer.MAX_PARTICLES;
        }

        while (snowBuffer.tail != snowBuffer.head && !snowBuffer.buffer[snowBuffer.tail].alive)
        {
            snowBuffer.tail = (snowBuffer.tail + 1) % snowBuffer.MAX_PARTICLES;
        }
    }

    private void DrawSnow()
    {
        int i = snowBuffer.tail;
        while (i != snowBuffer.head)
        {
            ParticleData p = snowBuffer.buffer[i];
            if (p.alive)
            {
                Graphics.DrawCircleV(p.position, p.radius, p.color);
            }
            i = (i + 1) % snowBuffer.MAX_PARTICLES;
        }
    }

    private void UpdateLightning()
    {
        lightningTimer += Time.GetFrameTime();

        if (lightningTimer > 3f && RandomHelper.Int(100) < 2)
        {
            showLightning = true;
            lightningTimer = 0f;
        }

        if (showLightning && lightningTimer > 0.2f)
        {
            showLightning = false;
        }
    }

    private void DrawLightning()
    {
        if (showLightning)
        {
            if (lightningTimer <= 0.1f)
            {
                Graphics.DrawRectangle(0, 0, screenWidth, screenHeight,
                    new Color(255, 255, 255, 150));
            }
            else
            {
                Graphics.DrawLineEx(
                    new Vector2(RandomHelper.Int(0, screenWidth), 0),
                    new Vector2(RandomHelper.Int(0, screenWidth), screenHeight),
                    2.0f,
                    Color.Gold
                );
            }
        }
    }

    private void DrawClouds()
    {
        float time = (float)Time.GetTime();
        int cloudY = 50;

        for (int I = 0; I < 3; I++)
        {
            cloudOffsets[I] += 0.3f;
            if (cloudOffsets[I] > screenWidth + 100)
            {
                cloudOffsets[I] = -100;
            }

            int x = (int)cloudOffsets[I];
            int y = cloudY + (I * 40);
            DrawCloud(x, y, new Color(200, 200, 210, 150));
        }
    }

    private void DrawCloud(int x, int y, Color color)
    {
        Graphics.DrawCircle(x, y, 30, color);
        Graphics.DrawCircle(x + 25, y, 35, color);
        Graphics.DrawCircle(x + 50, y, 30, color);
        Graphics.DrawCircle(x + 25, y - 20, 25, color);
    }

    private void DrawFog()
    {
        Graphics.DrawRectangle(0, 0, screenWidth, screenHeight,
            new Color(220, 220, 230, 80));

        float offset = MathF.Sin((float)Time.GetTime() * 0.5f) * 50;
        for (int I = 0; I < 5; I++)
        {
            int y = I * 150 + (int)offset;
            Graphics.DrawRectangle(0, y, screenWidth, 80,
                new Color(200, 200, 210, 40));
        }
    }
}
