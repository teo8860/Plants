/// <summary>
using System;
using System.Numerics;

namespace Engine.Tools;

/// <summary>
/// Helper per generazione numeri casuali
/// </summary>
public static class RandomHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// Genera un float casuale tra min e max
    /// </summary>
    public static float Float(float min, float max) => min + _random.NextSingle() * (max - min);

    /// <summary>
    /// Genera un int casuale tra min e max (escluso)
    /// </summary>
    public static int Int(int min = 0, int max = int.MaxValue) => _random.Next(min, max);

    /// <summary>
    /// Restituisce true con probabilità chance (0-1)
    /// </summary>
    public static bool Chance(float chance) => _random.NextSingle() < chance;

    /// <summary>
    /// Genera un punto casuale dentro un cerchio
    /// </summary>
    public static Vector2 InsideCircle(float radius)
    {
        float angle = _random.NextSingle() * MathF.PI * 2;
        float distance = _random.NextSingle() * radius;
        return new Vector2(MathF.Cos(angle) * distance, MathF.Sin(angle) * distance);
    }

    /// <summary>
    /// Sceglie un elemento casuale da una lista di elementi
    /// </summary>
    public static T Choose<T>(params T[] items) => items[_random.Next(items.Length)];
}