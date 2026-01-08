using System;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Plants;

/// <summary>
/// Helper per conversione coordinate mondo-schermo
/// Coordinate mondo: Y=0 al terreno, Y positivo verso l'alto
/// Coordinate schermo: Y=0 in alto, Y aumenta verso il basso
/// </summary>
public static class CoordinateHelper
{
    /// <summary>
    /// Posizione Y del terreno sullo schermo (in pixel dalla cima)
    /// </summary>
    public static float GroundScreenY => GameProperties.viewHeight - GameProperties.groundPosition;

    /// <summary>
    /// Converte Y da coordinate mondo a coordinate schermo
    /// </summary>
    public static float ToScreenY(float worldY, float cameraY)
    {
        return GroundScreenY + cameraY - worldY;
    }

    /// <summary>
    /// Converte un punto da coordinate mondo a coordinate schermo
    /// </summary>
    public static Vector2 ToScreen(Vector2 worldPos, float cameraY)
    {
        return new Vector2(worldPos.X, ToScreenY(worldPos.Y, cameraY));
    }

    /// <summary>
    /// Converte Y da coordinate schermo a coordinate mondo
    /// </summary>
    public static float ToWorldY(float screenY, float cameraY)
    {
        return GroundScreenY + cameraY - screenY;
    }
}

/// <summary>
/// Helper per numeri random
/// </summary>
public static class RandomHelper
{
    private static readonly Random _random = new();

    /// <summary>
    /// Float casuale nell'intervallo [min, max]
    /// </summary>
    public static float Float(float min, float max)
    {
        if (max < min)
            (min, max) = (max, min);

        return min + (float)_random.NextDouble() * (max - min);
    }

    /// <summary>
    /// Int casuale nell'intervallo [min, max)
    /// </summary>
    public static int Int(int min, int max) => _random.Next(min, max);

    /// <summary>
    /// Int casuale nell'intervallo [0, max)
    /// </summary>
    public static int Int(int max) => _random.Next(max);

    /// <summary>
    /// Bool casuale con probabilità specificata (0-100)
    /// </summary>
    public static bool Chance(int percent) => _random.Next(100) < percent;

    /// <summary>
    /// Vettore random in un cerchio
    /// </summary>
    public static Vector2 InsideCircle(float radius)
    {
        float angle = Float(0, MathF.PI * 2);
        float r = Float(0, radius);
        return new Vector2(MathF.Cos(angle) * r, MathF.Sin(angle) * r);
    }

    /// <summary>
    /// Sceglie un elemento random da un array
    /// </summary>
    public static T Choose<T>(params T[] options) => options[Int(options.Length)];
}

/// <summary>
/// Helper matematici
/// </summary>
public static class MathHelper
{
    public const float Deg2Rad = MathF.PI / 180f;
    public const float Rad2Deg = 180f / MathF.PI;

    /// <summary>
    /// Limita un valore in un intervallo
    /// </summary>
    public static float Clamp(float value, float min, float max) =>
        MathF.Max(min, MathF.Min(max, value));

    /// <summary>
    /// Interpolazione lineare
    /// </summary>
    public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp(t, 0, 1);

    /// <summary>
    /// Interpolazione lineare per vettori
    /// </summary>
    public static Vector2 Lerp(Vector2 a, Vector2 b, float t) =>
        new(Lerp(a.X, b.X, t), Lerp(a.Y, b.Y, t));

    /// <summary>
    /// Smooth step (ease in-out)
    /// </summary>
    public static float SmoothStep(float t)
    {
        t = Clamp(t, 0, 1);
        return t * t * (3 - 2 * t);
    }

    /// <summary>
    /// Angolo tra due punti in gradi
    /// </summary>
    public static float AngleBetween(Vector2 from, Vector2 to)
    {
        Vector2 delta = to - from;
        return MathF.Atan2(delta.Y, delta.X) * Rad2Deg;
    }

    /// <summary>
    /// Distanza tra due punti
    /// </summary>
    public static float Distance(Vector2 a, Vector2 b) => Vector2.Distance(a, b);
}

/// <summary>
/// Helper per salvataggio/caricamento dati
/// </summary>
public static class SaveHelper
{
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    /// <summary>
    /// Salva un oggetto su file JSON
    /// </summary>
    public static void Save<T>(string fileName, T data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, _jsonOptions);
            File.WriteAllText(GetSavePath(fileName), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel salvataggio: {ex.Message}");
        }
    }

    /// <summary>
    /// Salva un oggetto su file JSON (async)
    /// </summary>
    public static async Task SaveAsync<T>(string fileName, T data)
    {
        try
        {
            string json = JsonSerializer.Serialize(data, _jsonOptions);
            await File.WriteAllTextAsync(GetSavePath(fileName), json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel salvataggio async: {ex.Message}");
        }
    }

    /// <summary>
    /// Carica un oggetto da file JSON
    /// </summary>
    public static T? Load<T>(string fileName) where T : class
    {
        try
        {
            string path = GetSavePath(fileName);
            if (!File.Exists(path))
                return null;

            string json = File.ReadAllText(path);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel caricamento: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Carica un oggetto da file JSON (async)
    /// </summary>
    public static async Task<T?> LoadAsync<T>(string fileName) where T : class
    {
        try
        {
            string path = GetSavePath(fileName);
            if (!File.Exists(path))
                return null;

            string json = await File.ReadAllTextAsync(path);
            return JsonSerializer.Deserialize<T>(json, _jsonOptions);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel caricamento async: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Verifica se esiste un salvataggio
    /// </summary>
    public static bool Exists(string fileName) => File.Exists(GetSavePath(fileName));

    /// <summary>
    /// Elimina un salvataggio
    /// </summary>
    public static void Delete(string fileName)
    {
        string path = GetSavePath(fileName);
        if (File.Exists(path))
            File.Delete(path);
    }

    /// <summary>
    /// Ottiene il percorso completo per un file di salvataggio
    /// </summary>
    private static string GetSavePath(string fileName)
    {
        // Usa la cartella AppData su Windows, home directory altrove
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(baseDir))
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);

        string saveDir = Path.Combine(baseDir, "Plants");
        Directory.CreateDirectory(saveDir);

        return Path.Combine(saveDir, fileName);
    }
}

/// <summary>
/// Timer semplificato per eventi periodici
/// </summary>
public class GameTimer
{
    private float _elapsed = 0f;
    private readonly float _interval;
    private readonly Action _callback;
    private bool _isRunning = true;

    public GameTimer(float intervalSeconds, Action callback)
    {
        _interval = intervalSeconds;
        _callback = callback;
    }

    public void Update(float deltaTime)
    {
        if (!_isRunning) return;

        _elapsed += deltaTime;
        while (_elapsed >= _interval)
        {
            _elapsed -= _interval;
            _callback?.Invoke();
        }
    }

    public void Start() => _isRunning = true;
    public void Stop() => _isRunning = false;
    public void Reset() => _elapsed = 0f;
}