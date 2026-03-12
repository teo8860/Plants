using System;
using System.Text.Json;
using System.IO;

namespace Plants;

public class MinigameResult
{
    public TipoMinigioco Tipo { get; set; }
    public bool Vinto { get; set; }
    public int Punteggio { get; set; }
    public int PunteggioMassimo { get; set; }
    public int FoglieGuadagnate { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;

    private static string GetResultDir()
    {
        string baseDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        if (string.IsNullOrEmpty(baseDir))
            baseDir = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        string dir = Path.Combine(baseDir, "Plants", "minigame_results");
        Directory.CreateDirectory(dir);
        return dir;
    }

    public static string GetResultFilePath()
    {
        return Path.Combine(GetResultDir(), "last_result.json");
    }

    public void Save()
    {
        string json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(GetResultFilePath(), json);
    }

    public static MinigameResult? Load()
    {
        string path = GetResultFilePath();
        if (!File.Exists(path)) return null;
        try
        {
            string json = File.ReadAllText(path);
            var result = JsonSerializer.Deserialize<MinigameResult>(json);
            File.Delete(path); // consuma il risultato
            return result;
        }
        catch
        {
            return null;
        }
    }
}
