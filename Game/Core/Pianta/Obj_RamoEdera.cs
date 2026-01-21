using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Obj_RamoEdera: GameElement
{
    public float StartX, StartY;
    public float Direction;
    public List<Vector2> Punti = new();
    public List<(Vector2 pos, float size, float angle)> MiniFoglie = new();
    public float GrowthProgress = 0;
    public int MaxSegments = 12;
    public Color Colore;

    private Random rng;
    private float oscillationTime = 0;

    public Obj_RamoEdera(float x, float y, float direction, Color colore, int seed)
    {
        StartX = x;
        StartY = y;
        Direction = direction;
        Colore = colore;
        rng = new Random(seed);

        float curX = x;
        float curY = y;
        Punti.Add(new Vector2(curX, curY));

        for (int i = 0; i < MaxSegments; i++)
        {
            float dx = direction * (8 + (float)rng.NextDouble() * 6);
            float dy = (float)(rng.NextDouble() - 0.5) * 4;
            dy += MathF.Sin(i * 0.3f) * 1.5f;

            curX += dx;
            curY += dy;
            curX = Math.Clamp(curX, 8, GameProperties.cameraWidth - 8);

            Punti.Add(new Vector2(curX, curY));

            if (i % 2 == 0)
            {
                float leafAngle = (float)(rng.NextDouble() * MathF.PI - MathF.PI / 2);
                MiniFoglie.Add((new Vector2(curX, curY), 4 + (float)rng.NextDouble() * 2, leafAngle));
            }
        }
    }

    public override void Update()
    {
        float deltaTime = Time.GetFrameTime();
        oscillationTime += deltaTime;
        if (GrowthProgress < 1)
        {
            GrowthProgress += deltaTime * 0.4f;
            if (GrowthProgress > 1) GrowthProgress = 1;
        }
    }

    public override void Draw()
    {
        if (!ViewCulling.IsValueVisible(StartY, Rendering.camera.position.Y)) return;

        int visibleSegments = (int)(Punti.Count * GrowthProgress);
        if (visibleSegments < 2) return;

        for (int i = 0; i < visibleSegments - 1; i++)
        {
            Vector2 p1 = Punti[i];
            Vector2 p2 = Punti[i + 1];

            float osc = MathF.Sin(oscillationTime * 2 + i * 0.5f) * 1.2f;
            float thickness = 2.5f * (1 - (float)i / Punti.Count * 0.4f);

            Graphics.DrawLineEx(
                new Vector2(p1.X + osc, p1.Y),
                new Vector2(p2.X + osc, p2.Y),
                thickness, Colore);
        }

        int visibleLeaves = (int)(MiniFoglie.Count * GrowthProgress);
        for (int i = 0; i < visibleLeaves; i++)
        {
            var (pos, size, angle) = MiniFoglie[i];
            float leafOsc = MathF.Sin(oscillationTime * 2.5f + i) * 0.08f;
            DrawMiniLeaf(pos.X, pos.Y, size, angle + leafOsc, Colore);
        }
    }

    private void DrawMiniLeaf(float x, float y, float size, float angle, Color color)
    {
        float cos = MathF.Cos(angle);
        float sin = MathF.Sin(angle);

        Vector2 tip = new Vector2(x + cos * size, y - sin * size);
        Vector2 left = new Vector2(x - sin * size * 0.25f, y - cos * size * 0.25f);
        Vector2 right = new Vector2(x + sin * size * 0.25f, y + cos * size * 0.25f);

        Graphics.DrawTriangle(tip, left, right, color);
    }
}
