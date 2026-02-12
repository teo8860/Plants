/// <summary>
using System;
using System.Numerics;

namespace Plants;


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

    public static int DeterministicIntAt(int seed, int index)
    {
        ulong x = (ulong)seed;
        x += (ulong)index * 0x9E3779B97F4A7C15UL;

        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        x ^= x >> 31;

        return (int)(x & 0x7FFFFFFF);
    }

    public static int DeterministicIntRange(int seed, int index, int min, int max)
    {
        if (max <= min)
            return min;

        int r = DeterministicIntAt(seed, index);
        return min + (r % (max - min));
    }


    public static float DeterministicFloatAt(int seed, int index)
    {
        ulong x = Mix(seed, index);

        const float inv = 1.0f / (1u << 24);
        return (float)((x >> 40) & 0xFFFFFF) * inv;
    }

    public static float DeterministicFloatRangeAt(int seed, int index, float min, float max)
    {
        if (max <= min)
            return min;

        float t = DeterministicFloatAt(seed, index);
        return min + (max - min) * t;
    }

    private static ulong Mix(int seed, int index)
    {
        ulong x = (ulong)seed;
        x += (ulong)index * 0x9E3779B97F4A7C15UL;

        x = (x ^ (x >> 30)) * 0xBF58476D1CE4E5B9UL;
        x = (x ^ (x >> 27)) * 0x94D049BB133111EBUL;
        x ^= x >> 31;

        return x;
    }
}