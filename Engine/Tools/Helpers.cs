/// <summary>
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Threading.Tasks;

namespace Engine.Tools;

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

            string path = GetSavePath(fileName);

            File.WriteAllText(path, json);

            // Verifica
            if (File.Exists(path))
            {
                var size = new FileInfo(path).Length;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore nel salvataggio: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
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
/// Helper per coordinate e conversioni
/// </summary>
public static class CoordinateHelper
{
    /// <summary>
    /// Converte coordinate mondo a schermo (Y)
    /// </summary>
    public static float ToScreenY(float worldY, float cameraY) => worldY - cameraY;

    /// <summary>
    /// Converte coordinate mondo a schermo
    /// </summary>
    public static Vector2 ToScreen(Vector2 worldPos, Vector2 cameraPos) =>
        new(worldPos.X - cameraPos.X, worldPos.Y - cameraPos.Y);

    /// <summary>
    /// Converte coordinate schermo a mondo (Y)
    /// </summary>
    public static float ToWorldY(float screenY, float cameraY) => screenY + cameraY;

    /// <summary>
    /// Converte coordinate schermo a mondo
    /// </summary>
    public static Vector2 ToWorld(Vector2 screenPos, Vector2 cameraPos = default) =>
        new(screenPos.X + cameraPos.X, screenPos.Y + cameraPos.Y);
}

/// <summary>
/// Helper per funzioni matematiche
/// </summary>
public static class MathHelper
{
    public static float CalcoloMediaValori(List<float> valori)
	{
		if(valori.Count == 0)
			return float.NaN;

		var bins = new Dictionary<string, (double a, double b, int count)>();

		foreach(var v in valori)
		{
			if(v <= 0)
				throw new Exception("Valori <= 0 non supportati");

			int exp = (int)Math.Floor(Math.Log10(v));
			double a = Math.Pow(10, exp);
			double b = Math.Pow(10, exp + 1);
			string key = a + "-" + b;

			if(!bins.ContainsKey(key))
				bins[key] = (a, b, 0);

			bins[key] = (bins[key].a, bins[key].b, bins[key].count + 1);
		}

		double totale = valori.Count;
		double risultato = 0;

		foreach(var bin in bins.Values)
		{
			double peso = bin.count / totale;
			double centro = Math.Sqrt(bin.a * bin.b); // centro geometrico
			risultato += centro * peso;
		}

		return (float)risultato;
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