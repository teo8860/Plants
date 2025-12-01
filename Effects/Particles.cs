using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;
using System.Numerics;

namespace Plants;

public class Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public float radius;
    public Color color;
    public bool alive;
    public float lifetime;
}

public class CircularBuffer
{
    public const int MAX_PARTICLES = 3000;
    public int head;
    public int tail;
    public Particle[] buffer = new Particle[MAX_PARTICLES];

    public CircularBuffer()
    {
        for (int i = 0; i < MAX_PARTICLES; i++)
        {
            buffer[i] = new Particle();
        }
    }
}

public class WeatherParticleSystem : GameElement
{
    private readonly Random random = new Random();
    private CircularBuffer rainBuffer;
    private CircularBuffer snowBuffer;
    private int screenWidth;
    private int screenHeight;
    private float lightningTimer = 0f;
    private bool showLightning = false;
    private float[] cloudOffsets = new float[3];

    public WeatherParticleSystem()
    {
        rainBuffer = new CircularBuffer();
        snowBuffer = new CircularBuffer();

        for (int i = 0; i < cloudOffsets.Length; i++)
        {
            cloudOffsets[i] = random.Next(0, 300);
        }
    }

    public void Initialize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
    }

    private Particle AddToBuffer(CircularBuffer buffer)
    {
        if ((buffer.head + 1) % CircularBuffer.MAX_PARTICLES != buffer.tail)
        {
            Particle particle = buffer.buffer[buffer.head];
            buffer.head = (buffer.head + 1) % CircularBuffer.MAX_PARTICLES;
            return particle;
        }
        return null;
    }

    public void UpdateAndDrawWeather(Weather weather)
    {
        switch (weather)
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
            Particle newParticle = AddToBuffer(rainBuffer);
            if (newParticle != null)
            {
                newParticle.position = new Vector2(random.Next(0, screenWidth), -10);
                newParticle.alive = true;
                newParticle.radius = 2.0f;
                newParticle.color = new Color(173, 216, 230, heavy ? (byte)180 : (byte)120);
                newParticle.velocity = new Vector2(0, heavy ? 12 : 8);
            }
        }

        int i = rainBuffer.tail;
        while (i != rainBuffer.head)
        {
            Particle p = rainBuffer.buffer[i];
            if (p.alive)
            {
                p.position += p.velocity;

                if (p.position.Y > screenHeight || p.position.X > screenWidth)
                {
                    p.alive = false;
                }
            }
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }

        while (rainBuffer.tail != rainBuffer.head && !rainBuffer.buffer[rainBuffer.tail].alive)
        {
            rainBuffer.tail = (rainBuffer.tail + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }

    private void DrawRain()
    {
        int i = rainBuffer.tail;
        while (i != rainBuffer.head)
        {
            Particle p = rainBuffer.buffer[i];
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
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }

    private void UpdateSnow()
    {
        for (int I = 0; I < 3; I++)
        {
            Particle newParticle = AddToBuffer(snowBuffer);
            if (newParticle != null)
            {
                newParticle.position = new Vector2(random.Next(0, screenWidth), -10);
                newParticle.alive = true;
                newParticle.radius = 3.0f;
                newParticle.color = new Color(255, 255, 255, 200);
                newParticle.velocity = new Vector2(0, 1 + random.Next(2));
                newParticle.lifetime = 0;
            }
        }

        int i = snowBuffer.tail;
        while (i != snowBuffer.head)
        {
            Particle p = snowBuffer.buffer[i];
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
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }

        while (snowBuffer.tail != snowBuffer.head && !snowBuffer.buffer[snowBuffer.tail].alive)
        {
            snowBuffer.tail = (snowBuffer.tail + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }

    private void DrawSnow()
    {
        int i = snowBuffer.tail;
        while (i != snowBuffer.head)
        {
            Particle p = snowBuffer.buffer[i];
            if (p.alive)
            {
                Graphics.DrawCircleV(p.position, p.radius, p.color);
            }
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }

    private void UpdateLightning()
    {
        lightningTimer += Time.GetFrameTime();

        if (lightningTimer > 3f && random.Next(100) < 2)
        {
            showLightning = true;
            lightningTimer = 0f;
        }

        if (showLightning && lightningTimer > 0.1f)
        {
            showLightning = false;
        }
    }

    private void DrawLightning()
    {
        if (showLightning)
        {
            Graphics.DrawRectangle(0, 0, screenWidth, screenHeight,
                new Color(255, 255, 255, 100));
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

public class Water : GameElement
{
    private readonly Random random = new Random();
    private CircularBuffer buffer;
    private int screenWidth;
    private int screenHeight;

    public Water()
    {
    }

    public void Initialize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
        buffer = new CircularBuffer();
    }

    private Particle AddToCircularBuffer()
    {
        if ((buffer.head + 1) % CircularBuffer.MAX_PARTICLES != buffer.tail)
        {
            Particle particle = buffer.buffer[buffer.head];
            buffer.head = (buffer.head + 1) % CircularBuffer.MAX_PARTICLES;
            return particle;
        }
        return null;
    }

    public void EmitParticle(Vector2 emitterPosition)
    {
        Particle newParticle = AddToCircularBuffer();
        if (newParticle != null)
        {
            newParticle.position = emitterPosition;
            newParticle.alive = true;
            newParticle.radius = 5.0f;
            newParticle.color = Color.Blue;
            float speed = random.Next(8) / 5.0f;
            float direction = random.Next(360);
            newParticle.velocity = new Vector2(
                speed * MathF.Cos(direction * RayMath.Deg2Rad),
                speed * MathF.Sin(direction * RayMath.Deg2Rad)
            );
        }
    }

    public override void Update()
    {
        int i = buffer.tail;
        while (i != buffer.head)
        {
            Particle p = buffer.buffer[i];
            p.position.X += p.velocity.X;
            p.velocity.Y += 0.2f;
            p.position.Y += p.velocity.Y;
            Vector2 center = p.position;
            float radius = p.radius;
            if (center.X < -radius || center.X > screenWidth + radius ||
                center.Y < -radius || center.Y > screenHeight + radius)
            {
                p.alive = false;
            }
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }
        while (buffer.tail != buffer.head && !buffer.buffer[buffer.tail].alive)
        {
            buffer.tail = (buffer.tail + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }

    public override void Draw()
    {
        int i = buffer.tail;
        while (i != buffer.head)
        {
            Particle p = buffer.buffer[i];
            if (p.alive)
            {
                Graphics.DrawCircleV(p.position, p.radius, p.color);
            }
            i = (i + 1) % CircularBuffer.MAX_PARTICLES;
        }
    }
}