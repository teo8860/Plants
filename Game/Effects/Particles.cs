using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;
using System.Numerics;
using Engine.Tools;

namespace Plants;

public class ParticleData
{
    public Vector2 position;
    public Vector2 velocity;
    public Vector2 gravity_min;
    public Vector2 gravity_max;
    public Sprite image;
    public Color color;
    public float radius;
    public bool alive;
    public float lifetime = -1;
}

public class CircularBuffer
{
    public int MAX_PARTICLES = 3000;
    public int head;
    public int tail;
    public ParticleData[] buffer;

    public CircularBuffer(int max_particle = 3000)
    {
        this.MAX_PARTICLES = max_particle;
        this.buffer = new ParticleData[MAX_PARTICLES];

        for (int i = 0; i < max_particle; i++)
        {
            buffer[i] = new ParticleData();
        }
    }
}


public class Particle : GameElement
{
    private CircularBuffer buffer;
    private int screenWidth;
    private int screenHeight;
    public ParticleData defaultData;


    public void Initialize(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
        buffer = new CircularBuffer();
    }

    private ParticleData AddToCircularBuffer()
    {
        if ((buffer.head + 1) % this.buffer.MAX_PARTICLES != buffer.tail)
        {
            ParticleData particle = buffer.buffer[buffer.head];
            buffer.head = (buffer.head + 1) % this.buffer.MAX_PARTICLES;
            return particle;
        }
        return null;
    }

    public void EmitParticle(Vector2 emitterPosition)
    {
        ParticleData newParticle = AddToCircularBuffer();
        if (newParticle != null)
        {
            newParticle.position = emitterPosition;
            newParticle.alive = true;
            
            newParticle.lifetime = this.defaultData.lifetime;
            newParticle.radius = this.defaultData.radius;
            newParticle.color = this.defaultData.color;
            newParticle.velocity = this.defaultData.velocity;
            newParticle.gravity_min = this.defaultData.gravity_min;
            newParticle.gravity_max = this.defaultData.gravity_max;
        }
    }

    public override void Update()
    {
        int i = buffer.tail;
        while (i != buffer.head)
        {
            ParticleData p = buffer.buffer[i];

            if(p.lifetime > 0)
                p.lifetime -= 1;

            if(p.lifetime == 0)
            {
                p.lifetime = -1;
                p.alive = false;
                continue;
            }

            p.velocity.X += RandomHelper.Float(p.gravity_min.X, p.gravity_max.X);
            p.velocity.Y += RandomHelper.Float(p.gravity_min.Y, p.gravity_max.Y);

            p.position.X += p.velocity.X;
            p.position.Y += p.velocity.Y;

            Vector2 center = p.position;
            float radius = p.radius;
            if (center.X < -radius || center.X > screenWidth + radius ||
                center.Y < -radius+ Rendering.camera.position.Y  || center.Y > screenHeight + Rendering.camera.position.Y + radius)
            {
                p.alive = false;
            }
            i = (i + 1) % this.buffer.MAX_PARTICLES;
        }

        while (buffer.tail != buffer.head && !buffer.buffer[buffer.tail].alive)
        {
            buffer.tail = (buffer.tail + 1) % this.buffer.MAX_PARTICLES;
        }
    }

    public override void Draw()
    {
        int i = buffer.tail;
        while (i != buffer.head)
        {
            ParticleData p = buffer.buffer[i];
            if (p.alive)
            {
                if(p.image != null)
                    GameFunctions.DrawSprite(p.image, p.position, 0, 1);
                else
                    Graphics.DrawCircleV(p.position, p.radius, p.color);
            }
            i = (i + 1) % this.buffer.MAX_PARTICLES;
        }
    }
}
