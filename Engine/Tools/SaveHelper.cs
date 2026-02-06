/// <summary>
using System;
using System.IO;
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
