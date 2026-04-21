using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Obj_GuiStatsPanel : GameElement
{
    private int x, y;
    private bool expanded = true;

    // Tooltip oggetto
    private string tooltipItemId = null;

    // Indicatore stato hoverato
    private string hoveredStatId = null;
    private int hoveredStatDotY;

    // Layout indicatori
    private const int DOT_RADIUS = 3;
    private const int DOT_CX = 3;       // centro X del pallino relativo a x
    private const int LABEL_X = 11;     // inizio label relativo a x
    private const int BAR_X = 48;       // inizio barra relativo a x
    private const int BAR_W = 56;       // larghezza barra
    private const int VAL_X = 110;      // inizio valore relativo a x
    private const int PANEL_W = 146;

    private enum StatSeverity { Good, Warning, Bad, Critical }

    public Obj_GuiStatsPanel(int x, int y)
    {
        this.x = x;
        this.y = y;
        this.guiLayer = true;
    }

    private Color GetSeverityColor(StatSeverity s) => s switch
    {
        StatSeverity.Good     => new Color(80, 200, 80, 255),
        StatSeverity.Warning  => new Color(220, 200, 50, 255),
        StatSeverity.Bad      => new Color(230, 150, 50, 255),
        StatSeverity.Critical => new Color(220, 60, 60, 255),
        _                     => new Color(80, 200, 80, 255)
    };

    public override void Update()
    {
        tooltipItemId = null;
        hoveredStatId = null;

        if (!expanded) return;
        if (Game.pianta == null) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Hover sugli indicatori statistiche
        int yOff = y + 16;
        int lineH = 15;
        string[] statIds = { "salute", "idratazione", "energia", "ossigeno", "temperatura" };
        for (int i = 0; i < statIds.Length; i++)
        {
            int dotCenterX = x + DOT_CX;
            int dotCenterY = yOff + i * lineH + 5;
            int dx = mx - dotCenterX;
            int dy = my - dotCenterY;
            if (dx * dx + dy * dy <= (DOT_RADIUS + 3) * (DOT_RADIUS + 3))
            {
                hoveredStatId = statIds[i];
                hoveredStatDotY = dotCenterY;
                break;
            }
        }

        // Hover sugli oggetti equipaggiati
        if (Game.pianta.equippedItemIds == null) return;

        int iconSize = 18;
        int iconSpacing = 4;
        int iconsY = y + 100;
        int iconsX = x + 2;

        for (int i = 0; i < Game.pianta.equippedItemIds.Count; i++)
        {
            string id = Game.pianta.equippedItemIds[i];
            if (string.IsNullOrEmpty(id)) continue;

            int ix = iconsX + i * (iconSize + iconSpacing);
            if (mx >= ix && mx <= ix + iconSize && my >= iconsY && my <= iconsY + iconSize)
            {
                tooltipItemId = id;
                break;
            }
        }
    }

    public override void Draw()
    {
        var stats = Game.pianta.Stats;

        Color bg = Color.Black;
        bg.A = 180;
        int panelHeight = expanded ? 130 : 25;
        Graphics.DrawRectangleRounded(
            new Rectangle(x - 5, y - 5, PANEL_W, panelHeight),
            0.2f, 16, bg
        );

        int stage = WorldManager.GetCurrentStage();
        float diff = WorldManager.GetDifficultyMultiplier(stage);
        Graphics.DrawText($"Stage {stage} x{diff:F2}", x + 5, y + 2, 9, new Color(192, 192, 192, 255));

        if (!expanded) return;

        int yOff = y + 16;
        int lineH = 15;

        DrawStatBarWithIndicator("salute", "Salute", stats.Salute, Color.Red, ref yOff, lineH);
        DrawStatBarWithIndicator("idratazione", "Idrat.", stats.Idratazione, Color.Blue, ref yOff, lineH);
        DrawStatBarWithIndicator("energia", "Energia", stats.Metabolismo, Color.Yellow, ref yOff, lineH);
        DrawStatBarWithIndicator("ossigeno", "O2", stats.Ossigeno, Color.SkyBlue, ref yOff, lineH);

        Color tempColor = GetTemperatureColor(stats.Temperatura);
        DrawTemperatureLineWithIndicator(stats.Temperatura, tempColor, ref yOff, lineH);

        // Icone oggetti equipaggiati
        DrawEquippedItems();

        // Tooltip (disegnati per ultimi, sopra tutto)
        DrawStatTooltip();
        DrawItemTooltip();

        yOff += 5;
    }

    // ─── Statistiche con indicatore ─────────────────────────────

    private void DrawStatBarWithIndicator(string id, string label, float value, Color barColor, ref int y, int h)
    {
        StatSeverity severity = GetStatSeverity(id, value);
        Color dotColor = GetSeverityColor(severity);

        int cx = x + DOT_CX;
        int cy = y + 5;

        // Alone su hover
        if (hoveredStatId == id)
        {
            Color halo = dotColor;
            halo.A = 60;
            Graphics.DrawCircleV(new Vector2(cx, cy), DOT_RADIUS + 2, halo);
        }
        Graphics.DrawCircleV(new Vector2(cx, cy), DOT_RADIUS, dotColor);

        Graphics.DrawText(label, x + LABEL_X, y, 9, Color.LightGray);

        Graphics.DrawRectangle(x + BAR_X, y + 2, BAR_W, 8, Color.DarkGray);
        Graphics.DrawRectangle(x + BAR_X, y + 2, (int)(BAR_W * Math.Clamp(value, 0f, 1f)), 8, barColor);

        Graphics.DrawText($"{value:P0}", x + VAL_X, y, 9, Color.White);
        y += h;
    }

    private void DrawTemperatureLineWithIndicator(float temp, Color color, ref int y, int h)
    {
        StatSeverity severity = GetTemperatureSeverity(temp);
        Color dotColor = GetSeverityColor(severity);

        int cx = x + DOT_CX;
        int cy = y + 5;

        if (hoveredStatId == "temperatura")
        {
            Color halo = dotColor;
            halo.A = 60;
            Graphics.DrawCircleV(new Vector2(cx, cy), DOT_RADIUS + 2, halo);
        }
        Graphics.DrawCircleV(new Vector2(cx, cy), DOT_RADIUS, dotColor);

        Graphics.DrawText("Temp.", x + LABEL_X, y, 9, Color.LightGray);
        Graphics.DrawText($"{temp:F1}°C", x + BAR_X, y, 9, color);

        string desc = GetTemperatureDescription(temp);
        Graphics.DrawText(desc, x + BAR_X + 42, y, 9, color);
        y += h;
    }

    // ─── Severita' per stat ─────────────────────────────────────

    private StatSeverity GetStatSeverity(string statId, float value) => statId switch
    {
        "salute" => value < 0.10f ? StatSeverity.Critical
                  : value < 0.20f ? StatSeverity.Bad
                  : value < 0.50f ? StatSeverity.Warning
                  : StatSeverity.Good,

        "idratazione" => value < 0.01f ? StatSeverity.Critical
                       : value < 0.10f ? StatSeverity.Bad
                       : value < 0.30f ? StatSeverity.Warning
                       : StatSeverity.Good,

        "energia" => value < 0.05f ? StatSeverity.Critical
                   : value < ClimateDefinitions.SOGLIA_FAME_ENERGIA ? StatSeverity.Bad
                   : value < 0.30f ? StatSeverity.Warning
                   : StatSeverity.Good,

        "ossigeno" => value < 0.05f ? StatSeverity.Critical
                    : value < ClimateDefinitions.SOGLIA_SOFFOCAMENTO ? StatSeverity.Bad
                    : value < 0.30f ? StatSeverity.Warning
                    : StatSeverity.Good,

        _ => StatSeverity.Good
    };

    private StatSeverity GetTemperatureSeverity(float temp)
    {
        if (temp >= ClimateDefinitions.TEMPERATURA_IDEALE_MIN &&
            temp <= ClimateDefinitions.TEMPERATURA_IDEALE_MAX)
            return StatSeverity.Good;

        if (temp <= ClimateDefinitions.TEMPERATURA_GELIDA ||
            temp >= ClimateDefinitions.TEMPERATURA_TORRIDA)
            return StatSeverity.Critical;

        if (temp < ClimateDefinitions.TEMPERATURA_FREDDA ||
            temp > ClimateDefinitions.TEMPERATURA_CALDA)
            return StatSeverity.Bad;

        return StatSeverity.Warning;
    }

    // ─── Tooltip effetti stato ───────────────────────────────────

    private void DrawStatTooltip()
    {
        if (string.IsNullOrEmpty(hoveredStatId)) return;
        if (Game.pianta?.proprieta == null) return;

        var lines = BuildEffectLines(hoveredStatId);
        if (lines.Count == 0) return;

        int fontSize = 7;
        int lineH = fontSize + 4;
        int pad = 6;
        int tw = 155;
        int th = pad * 2 + lines.Count * lineH - 2;

        // Posizione: a sinistra del pannello
        int tx = x - 5 - tw - 4;
        int ty = hoveredStatDotY - th / 2;

        if (tx < 2) tx = 2;
        if (ty < 2) ty = 2;
        if (ty + th > Rendering.camera.screenHeight - 2)
            ty = Rendering.camera.screenHeight - th - 2;

        Color tooltipBg = new Color(20, 20, 32, 240);
        Color border = new Color(70, 70, 100, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(tx - 2, ty - 2, tw + 4, th + 4),
            0.15f, 6, border);
        Graphics.DrawRectangleRounded(
            new Rectangle(tx, ty, tw, th),
            0.15f, 6, tooltipBg);

        int ly = ty + pad;
        foreach (var (text, color) in lines)
        {
            Graphics.DrawText(text, tx + pad, ly, fontSize, color);
            ly += lineH;
        }
    }

    private List<(string text, Color color)> BuildEffectLines(string statId)
    {
        var lines = new List<(string, Color)>();
        var stats = Game.pianta.Stats;
        var logic = Game.pianta.proprieta;

        Color white = new Color(200, 200, 210, 255);
        Color good  = new Color(80, 200, 80, 255);
        Color warn  = new Color(220, 200, 50, 255);
        Color bad   = new Color(230, 150, 50, 255);
        Color crit  = new Color(220, 80, 80, 255);

        switch (statId)
        {
            case "salute":
                BuildSaluteEffects(lines, logic, good, warn, bad, crit);
                break;
            case "idratazione":
                BuildIdratazioneEffects(lines, stats, good, warn, bad, crit);
                break;
            case "energia":
                BuildEnergiaEffects(lines, stats, good, warn, bad, crit);
                break;
            case "ossigeno":
                BuildOssigenoEffects(lines, stats, good, warn, bad, crit);
                break;
            case "temperatura":
                BuildTemperaturaEffects(lines, stats, good, warn, bad, crit, white);
                break;
        }

        return lines;
    }

    private void BuildSaluteEffects(List<(string, Color)> l,
        GameLogicPianta logic, Color good, Color warn, Color bad, Color crit)
    {
        float pct = logic.PercentualeSalute;

        if (pct >= 0.50f)
        {
            l.Add(("Salute buona", good));
            l.Add(("Nessun effetto negativo", good));
        }
        else if (pct >= ClimateDefinitions.SOGLIA_CRITICA_SALUTE)
        {
            int g = (int)(pct * 2f * 100f);
            l.Add(("Salute ridotta", warn));
            l.Add(($"Crescita rallentata: ~{g}%", warn));
        }
        else if (pct >= 0.10f)
        {
            int g = (int)(pct * 2f * 100f);
            l.Add(("Salute critica!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            l.Add(("Caduta foglie: x1.8", bad));
        }
        else
        {
            l.Add(("Pericolo di morte!", crit));
            l.Add(("Crescita quasi ferma", crit));
            l.Add(("Caduta foglie: x1.8", crit));
        }

        if (logic.stats.Infestata)
        {
            int inf = (int)(logic.stats.IntensitaInfestazione * 100f);
            l.Add(($"Parassiti attivi: {inf}%", bad));
        }
    }

    private void BuildIdratazioneEffects(List<(string, Color)> l,
        PlantStats stats, Color good, Color warn, Color bad, Color crit)
    {
        float v = stats.Idratazione;

        if (v >= 0.30f)
        {
            l.Add(("Idratazione buona", good));
            l.Add(("Nessun effetto negativo", good));
        }
        else if (v >= 0.10f)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Idratazione bassa", warn));
            l.Add(($"Crescita rallentata: ~{g}%", warn));
        }
        else if (v >= 0.01f)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Idratazione molto bassa!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            if (v < ClimateDefinitions.SOGLIA_DISIDRATAZIONE)
            {
                l.Add(("Danno da siccita'", bad));
                l.Add(("Caduta foglie: x2", bad));
                l.Add(("Parassiti: +30%", bad));
            }
        }
        else
        {
            l.Add(("Disidratazione totale!", crit));
            l.Add(("Crescita: bloccata", crit));
            l.Add(("Danno da siccita'", crit));
            l.Add(("Caduta foglie: x2", crit));
            l.Add(("Parassiti: +30%", crit));
        }
    }

    private void BuildEnergiaEffects(List<(string, Color)> l,
        PlantStats stats, Color good, Color warn, Color bad, Color crit)
    {
        float v = stats.Metabolismo;

        if (v >= 0.30f)
        {
            l.Add(("Energia buona", good));
            l.Add(("Nessun effetto negativo", good));
        }
        else if (v >= ClimateDefinitions.SOGLIA_FAME_ENERGIA)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Energia bassa", warn));
            l.Add(($"Crescita rallentata: ~{g}%", warn));
        }
        else if (v >= 0.01f)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Fame energetica!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            l.Add(("Danno in corso", bad));
        }
        else
        {
            l.Add(("Fame critica!", crit));
            l.Add(("Crescita: bloccata", crit));
            l.Add(("Danno grave", crit));
        }
    }

    private void BuildOssigenoEffects(List<(string, Color)> l,
        PlantStats stats, Color good, Color warn, Color bad, Color crit)
    {
        float v = stats.Ossigeno;

        if (v >= 0.30f)
        {
            l.Add(("Ossigeno buono", good));
            l.Add(("Nessun effetto negativo", good));
        }
        else if (v >= ClimateDefinitions.SOGLIA_SOFFOCAMENTO)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Ossigeno basso", warn));
            l.Add(($"Crescita rallentata: ~{g}%", warn));
        }
        else if (v >= 0.01f)
        {
            int g = (int)(v / 0.30f * 100f);
            l.Add(("Soffocamento!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            l.Add(("Danno da mancanza d'aria", bad));
        }
        else
        {
            l.Add(("Soffocamento critico!", crit));
            l.Add(("Crescita: bloccata", crit));
            l.Add(("Danno grave", crit));
        }
    }

    private void BuildTemperaturaEffects(List<(string, Color)> l,
        PlantStats stats, Color good, Color warn, Color bad, Color crit, Color white)
    {
        float t = stats.Temperatura;

        if (t >= ClimateDefinitions.TEMPERATURA_IDEALE_MIN &&
            t <= ClimateDefinitions.TEMPERATURA_IDEALE_MAX)
        {
            l.Add(("Temperatura ideale", good));
            l.Add(("Crescita: ottimale", good));
            l.Add(("Rigenerazione: +30%", good));
        }
        else if (t <= ClimateDefinitions.TEMPERATURA_GELIDA)
        {
            l.Add(("Gelo estremo!", crit));
            l.Add(("Crescita: ~5%", crit));
            l.Add(("Danno da gelo severo", crit));
            l.Add(("Consumo acqua: -50%", white));
            l.Add(("Fotosintesi: -50%", crit));
            l.Add(("Caduta foglie: x4", crit));
            l.Add(("Rigenerazione: bloccata", crit));
        }
        else if (t < ClimateDefinitions.TEMPERATURA_FREDDA)
        {
            int g = (int)(CalcolaGrowthPercent(t) * 100f);
            l.Add(("Freddo — danno!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            l.Add(("Danno da freddo", bad));
            l.Add(("Consumo acqua: -50%", white));
            l.Add(("Fotosintesi: -50%", bad));
            l.Add(("Caduta foglie: x2", bad));
            l.Add(("Rigenerazione: bloccata", bad));
        }
        else if (t < ClimateDefinitions.TEMPERATURA_FRESCA)
        {
            int g = (int)(CalcolaGrowthPercent(t) * 100f);
            l.Add(("Fresco", warn));
            l.Add(($"Crescita: ~{g}%", warn));
            l.Add(("Consumo acqua: -30%", white));
        }
        else if (t < ClimateDefinitions.TEMPERATURA_IDEALE_MIN)
        {
            int g = (int)(CalcolaGrowthPercent(t) * 100f);
            l.Add(("Sotto l'ideale", warn));
            l.Add(($"Crescita: ~{g}%", warn));
            l.Add(("Consumo acqua: -30%", white));
        }
        else if (t <= ClimateDefinitions.TEMPERATURA_CALDA)
        {
            int g = (int)(CalcolaGrowthPercent(t) * 100f);
            l.Add(("Sopra l'ideale", warn));
            l.Add(($"Crescita: ~{g}%", warn));
            l.Add(("Consumo acqua: +30%", warn));
        }
        else if (t < ClimateDefinitions.TEMPERATURA_TORRIDA)
        {
            int g = (int)(CalcolaGrowthPercent(t) * 100f);
            l.Add(("Caldo — danno!", bad));
            l.Add(($"Crescita: ~{g}%", bad));
            l.Add(("Danno da calore", bad));
            l.Add(("Consumo acqua: +80%", bad));
            l.Add(("Fotosintesi: -30%", bad));
            l.Add(("Caduta foglie: x1.5", bad));
            l.Add(("Rigenerazione: bloccata", bad));
        }
        else
        {
            l.Add(("Calore estremo!", crit));
            l.Add(("Crescita: <20%", crit));
            l.Add(("Danno da calore severo", crit));
            l.Add(("Consumo acqua: +150%", crit));
            l.Add(("Fotosintesi: -70%", crit));
            l.Add(("Caduta foglie: x3.5", crit));
            l.Add(("Rigenerazione: bloccata", crit));
        }
    }

    /// <summary>Moltiplicatore crescita semplificato (senza componente random).</summary>
    private float CalcolaGrowthPercent(float temp)
    {
        if (temp <= ClimateDefinitions.TEMPERATURA_GELIDA) return 0f;
        if (temp < ClimateDefinitions.TEMPERATURA_FREDDA)
        {
            float n = (temp - ClimateDefinitions.TEMPERATURA_GELIDA) /
                      (ClimateDefinitions.TEMPERATURA_FREDDA - ClimateDefinitions.TEMPERATURA_GELIDA);
            return 0.1f + n * 0.3f;
        }
        if (temp < ClimateDefinitions.TEMPERATURA_FRESCA)
        {
            float n = (temp - ClimateDefinitions.TEMPERATURA_FREDDA) /
                      (ClimateDefinitions.TEMPERATURA_FRESCA - ClimateDefinitions.TEMPERATURA_FREDDA);
            return 0.4f + n * 0.3f;
        }
        if (temp >= ClimateDefinitions.TEMPERATURA_IDEALE_MIN &&
            temp <= ClimateDefinitions.TEMPERATURA_IDEALE_MAX)
            return 1.0f;
        if (temp < ClimateDefinitions.TEMPERATURA_IDEALE_MIN)
        {
            float n = (temp - ClimateDefinitions.TEMPERATURA_FRESCA) /
                      (ClimateDefinitions.TEMPERATURA_IDEALE_MIN - ClimateDefinitions.TEMPERATURA_FRESCA);
            return 0.7f + n * 0.3f;
        }
        if (temp <= ClimateDefinitions.TEMPERATURA_CALDA)
        {
            float n = (temp - ClimateDefinitions.TEMPERATURA_IDEALE_MAX) /
                      (ClimateDefinitions.TEMPERATURA_CALDA - ClimateDefinitions.TEMPERATURA_IDEALE_MAX);
            return 1.0f - n * 0.3f;
        }
        if (temp < ClimateDefinitions.TEMPERATURA_TORRIDA)
        {
            float n = (temp - ClimateDefinitions.TEMPERATURA_CALDA) /
                      (ClimateDefinitions.TEMPERATURA_TORRIDA - ClimateDefinitions.TEMPERATURA_CALDA);
            return 0.7f - n * 0.5f;
        }
        return Math.Max(0f, 0.2f - (temp - ClimateDefinitions.TEMPERATURA_TORRIDA) * 0.02f);
    }

    // ─── Oggetti equipaggiati ────────────────────────────────────

    private void DrawEquippedItems()
    {
        if (Game.pianta == null || Game.pianta.equippedItemIds == null) return;

        int iconSize = 18;
        int iconSpacing = 4;
        int iconsY = y + 100;
        int iconsX = x + 2;

        bool hasAny = false;
        for (int i = 0; i < Game.pianta.equippedItemIds.Count; i++)
        {
            string id = Game.pianta.equippedItemIds[i];
            if (string.IsNullOrEmpty(id)) continue;
            hasAny = true;

            var def = ItemRegistry.Get(id);
            if (def == null) continue;

            int ix = iconsX + i * (iconSize + iconSpacing);

            Color iconBg = tooltipItemId == id
                ? new Color(100, 180, 255, 200)
                : new Color(60, 60, 80, 200);
            Color iconBorder = tooltipItemId == id
                ? new Color(140, 200, 255, 255)
                : new Color(90, 90, 110, 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(ix, iconsY, iconSize, iconSize),
                0.25f, 4, iconBg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(ix, iconsY, iconSize, iconSize),
                0.25f, 4, 1, iconBorder);

            string initial = def.Name.Length > 0 ? def.Name.Substring(0, 1) : "?";
            Graphics.DrawText(initial, ix + iconSize / 2 - 3, iconsY + iconSize / 2 - 5, 10, new Color(240, 240, 255, 255));
        }

        if (!hasAny)
        {
            Graphics.DrawText("No oggetti", iconsX, iconsY + 3, 7, new Color(120, 120, 140, 180));
        }
    }

    private void DrawItemTooltip()
    {
        if (string.IsNullOrEmpty(tooltipItemId)) return;

        var def = ItemRegistry.Get(tooltipItemId);
        if (def == null) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        int tw = 140;
        int th = 80;
        int tx = mx - tw - 8;
        int ty = my - th / 2;

        // Clamp sullo schermo
        if (tx < 2) tx = mx + 12;
        if (ty < 2) ty = 2;
        if (ty + th > Rendering.camera.screenHeight - 2)
            ty = Rendering.camera.screenHeight - th - 2;

        Color tooltipBg = new Color(25, 25, 38, 245);
        Color tooltipBorder = new Color(80, 80, 110, 255);
        Color textAccent = new Color(100, 180, 255, 255);
        Color textDim = new Color(170, 170, 195, 255);

        Graphics.DrawRectangleRounded(
            new Rectangle(tx - 2, ty - 2, tw + 4, th + 4),
            0.1f, 6, tooltipBorder);
        Graphics.DrawRectangleRounded(
            new Rectangle(tx, ty, tw, th),
            0.1f, 6, tooltipBg);

        Graphics.DrawText(def.Name, tx + 6, ty + 5, 10, textAccent);

        string catLabel = def.Category switch
        {
            ItemCategory.Cosmetico => "Cosmetico",
            ItemCategory.Consumabile => "Consumabile",
            _ => "Equipaggiabile"
        };
        Graphics.DrawText(catLabel, tx + 6, ty + 18, 7, textDim);

        Graphics.DrawLine(tx + 6, ty + 27, tx + tw - 6, ty + 27, tooltipBorder);

        DrawWrappedText(def.Description, tx + 6, ty + 31, tw - 12, 7, textDim);
    }

    // ─── Helpers ─────────────────────────────────────────────────

    private Color GetTemperatureColor(float temp)
    {
        if (temp <= 0)
            return new Color(100, 150, 255, 255);
        if (temp < 10)
            return new Color(150, 200, 255, 255);
        if (temp < 18)
            return new Color(200, 230, 255, 255);
        if (temp <= 25)
            return new Color(100, 255, 100, 255);
        if (temp <= 30)
            return new Color(255, 255, 100, 255);
        if (temp < 38)
            return new Color(255, 180, 100, 255);
        return new Color(255, 100, 100, 255);
    }

    private string GetTemperatureDescription(float temp)
    {
        if (temp <= 0) return "Gelido";
        if (temp < 10) return "Freddo";
        if (temp < 15) return "Fresco";
        if (temp < 18) return "Mite";
        if (temp <= 25) return "Ideale";
        if (temp <= 30) return "Caldo";
        if (temp < 38) return "Torrido";
        return "Estremo";
    }

    private void DrawWrappedText(string text, int x, int y, int maxWidth, int fontSize, Color color)
    {
        int charWidth = Math.Max(1, fontSize - 2);
        int maxChars = Math.Max(1, maxWidth / charWidth);
        string[] words = text.Split(' ');
        string line = "";
        int lineY = y;

        foreach (var word in words)
        {
            if ((line + " " + word).Trim().Length > maxChars)
            {
                Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
                lineY += fontSize + 3;
                line = word;
            }
            else
            {
                line = line.Length > 0 ? line + " " + word : word;
            }
        }

        if (line.Trim().Length > 0)
            Graphics.DrawText(line.Trim(), x, lineY, fontSize, color);
    }
}
