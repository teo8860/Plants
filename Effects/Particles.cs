using Plants;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Windowing;
using System;
using System.Numerics;
using System.Xml.Linq;


namespace Plants;


public class Particle
{
    public Vector2 position;
    public Vector2 velocity;
    public float radius;
    public Color color;
    public bool alive;
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

public class Water: GameElement
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

    public override  void Update()
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
