using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

/// <summary>
/// Utility statica per disegnare le statistiche di un seme con barre colorate.
/// Usata sia nel pannello dettagli inventario che nella schermata di piantaggio.
/// </summary>
public static class SeedStatsDrawer
{
    private static readonly Color barBgColor = new Color(30, 30, 25, 200);
    private static readonly Color textColor = new Color(230, 220, 200, 255);
    private static readonly Color labelColor = new Color(180, 170, 150, 255);

    private struct StatEntry
    {
        public string Label;
        public string ShortLabel;
        public float Value;
        public float MinVal;
        public float MaxVal;
        public Color BarColor;
    }

    /// <summary>
    /// Disegna tutte le statistiche del seme in un'area rettangolare.
    /// </summary>
    /// <param name="stats">Le statistiche del seme</param>
    /// <param name="x">Coordinata X dell'area</param>
    /// <param name="y">Coordinata Y dell'area</param>
    /// <param name="width">Larghezza dell'area</param>
    /// <param name="compact">Se true, usa layout compatto (per piantaggio)</param>
    /// <returns>Altezza totale occupata</returns>
    public static int Draw(SeedStats stats, int x, int y, int width, bool compact = false)
    {
        if (stats == null) return 0;

        // Scala unificata 0-99 (SeedStatScaling).
        var entries = new StatEntry[]
        {
            new() 
            { 
                Label = "Vitalità", 
                ShortLabel = "VIT", 
                Value = stats.vitalita, 
                MinVal = SeedStatScaling.StatMin, 
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(220, 60, 60, 255) 
            },
            new() 
            { 
                Label = "Idratazione",
                ShortLabel = "IDR",
                Value = stats.idratazione,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(60, 140, 220, 255) 
             },
            new() 
            { 
                Label = "Metabolismo",
                ShortLabel = "MET",
                Value = stats.metabolismo,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(220, 180, 50, 255)
            },
            new() 
            { 
                Label = "Vegetazione",
                ShortLabel = "VEG",
                Value = stats.vegetazione,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(60, 200, 80, 255)
            },
            new() 
            { 
                Label = "Res. Freddo",
                ShortLabel = "FRD",
                Value = stats.resistenzaFreddo,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(120, 200, 255, 255)
            },
            new() 
            { 
                Label = "Res. Caldo",
                ShortLabel = "CLD",
                Value = stats.resistenzaCaldo,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(255, 130, 50, 255) 
            },
            new() 
            { 
                Label = "Res. Parassiti",
                ShortLabel = "PAR",
                Value = stats.resistenzaParassiti,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(180, 100, 220, 255) 
            },
            new() 
            { 
                Label = "Res. Vuoto",
                ShortLabel = "VUO",
                Value = stats.resistenzaVuoto,
                MinVal = SeedStatScaling.StatMin,
                MaxVal = SeedStatScaling.StatMax,
                BarColor = new Color(160, 160, 200, 255) 
            },
        };

        if (compact)
            return DrawCompact(entries, x, y, width);
        else
            return DrawFull(entries, x, y, width);
    }

    private static int DrawFull(StatEntry[] entries, int x, int y, int width)
    {
        int rowHeight = 14;
        int barHeight = 5;
        int spacing = 2;
        int totalHeight = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            int rowY = y + i * (rowHeight + barHeight + spacing);

            // Label e valore
            Graphics.DrawText(entry.Label, x, rowY, 9, labelColor);

            string valText = FormatValue(entry.Value);
            int valW = valText.Length * 5;
            Graphics.DrawText(valText, x + width - valW, rowY, 9, textColor);

            // Barra
            int barY = rowY + 11;
            int barW = width;

            Graphics.DrawRectangleRounded(
                new Rectangle(x, barY, barW, barHeight),
                0.5f, 4, barBgColor);

            float ratio = GetRatio(entry.Value, entry.MinVal, entry.MaxVal);
            int fillW = Math.Max(1, (int)(barW * ratio));

            Graphics.DrawRectangleRounded(
                new Rectangle(x, barY, fillW, barHeight),
                0.5f, 4, entry.BarColor);

            totalHeight = (rowY + rowHeight + barHeight + spacing) - y;
        }

        return totalHeight;
    }

    private static int DrawCompact(StatEntry[] entries, int x, int y, int width)
    {
        int columns = 2;
        int colWidth = (width - 8) / columns;
        int rowHeight = 16;
        int barHeight = 4;
        int spacing = 3;
        int totalHeight = 0;

        for (int i = 0; i < entries.Length; i++)
        {
            var entry = entries[i];
            int col = i % columns;
            int row = i / columns;

            int cellX = x + col * (colWidth + 8);
            int cellY = y + row * (rowHeight + barHeight + spacing);

            // Short label e valore
            Graphics.DrawText(entry.ShortLabel, cellX, cellY, 8, labelColor);

            string valText = FormatValue(entry.Value);
            int valW = valText.Length * 4;
            Graphics.DrawText(valText, cellX + colWidth - valW, cellY, 8, GetValueColor(entry.Value, entry.MinVal, entry.MaxVal));

            // Barra
            int barY = cellY + 10;
            Graphics.DrawRectangleRounded(
                new Rectangle(cellX, barY, colWidth, barHeight),
                0.5f, 4, barBgColor);

            float ratio = GetRatio(entry.Value, entry.MinVal, entry.MaxVal);
            int fillW = Math.Max(1, (int)(colWidth * ratio));

            Graphics.DrawRectangleRounded(
                new Rectangle(cellX, barY, fillW, barHeight),
                0.5f, 4, entry.BarColor);

            totalHeight = (cellY + rowHeight + barHeight + spacing) - y;
        }

        return totalHeight;
    }

    private static float GetRatio(float value, float min, float max)
    {
        return Math.Clamp((value - min) / (max - min), 0f, 1f);
    }

    private static string FormatValue(float value)
    {
        // Scala 0-99: visualizzazione intera (sempre 2 cifre max).
        return ((int)Math.Round(value)).ToString();
    }

    private static Color GetValueColor(float value, float min, float max)
    {
        float ratio = GetRatio(value, min, max);

        if (ratio >= 0.7f) return new Color(100, 220, 100, 255);
        if (ratio >= 0.4f) return new Color(220, 200, 100, 255);
        if (ratio >= 0.2f) return new Color(220, 150, 80, 255);
        return new Color(220, 80, 80, 255);
    }
}
