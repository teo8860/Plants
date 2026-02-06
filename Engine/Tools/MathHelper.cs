/// <summary>
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Engine.Tools;

/// <summary>
/// Helper per funzioni matematiche
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// Trasformazione logaritmica con segno.
    /// Mantiene la progressione esponenziale e supporta valori negativi.
    /// </summary>
    private static double SignedLog(double value, double malusWeight)
    {
        if (value == 0)
            return 0;

        double weight = value < 0 ? malusWeight : 1.0;
        return Math.Sign(value) * Math.Log10(1 + Math.Abs(value)) * weight;
    }

    /// <summary>
    /// Trasformazione inversa della SignedLog.
    /// </summary>
    private static double InverseSignedLog(double value)
    {
        if (value == 0)
            return 0;

        return Math.Sign(value) * (Math.Pow(10, Math.Abs(value)) - 1);
    }

    /// <summary>
    /// Calcola una statistica bilanciata per pack opening / loot generation.
    /// </summary>
    public static float CalcoloMediaValori(
        List<float> valori,
        double malusWeight = 1.0)
    {
        if (valori == null || valori.Count == 0)
            return 0f;

        double sommaLog = 0;

        foreach (var v in valori)
            sommaLog += SignedLog(v, malusWeight);

        double mediaLog = sommaLog / valori.Count;

        return (float)InverseSignedLog(mediaLog);
    }

    /// <summary>
    /// Limita un valore tra min e max
    /// </summary>
    public static float Clamp(float value, float min, float max) =>
        value < min ? min : value > max ? max : value;

    /// <summary>
    /// Interpolazione lineare
    /// </summary>
    public static float Lerp(float a, float b, float t) => a + (b - a) * t;

    /// <summary>
    /// Interpolazione smooth step
    /// </summary>
    public static float SmoothStep(float a, float b, float t)
    {
        t = Clamp((t - a) / (b - a), 0, 1);
        return t * t * (3 - 2 * t);
    }

    /// <summary>
    /// Calcola l'angolo tra due vettori
    /// </summary>
    public static float AngleBetween(Vector2 a, Vector2 b)
    {
        float dot = Vector2.Dot(a, b);
        float magA = a.Length();
        float magB = b.Length();
        return MathF.Acos(dot / (magA * magB));
    }

    /// <summary>
    /// Calcola la distanza tra due punti
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b) => Vector2.Distance(a, b);
}
