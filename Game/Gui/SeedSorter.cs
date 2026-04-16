using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public enum SeedSortCriterion
{
    Default,
    Rarita,
    Vitalita,
    Idratazione,
    Metabolismo,
    Vegetazione,
    ResFreddo,
    ResCaldo,
    ResParassiti,
    ResVuoto
}

/// <summary>
/// Controllo di ordinamento dei semi riutilizzabile.
/// Offre un piccolo pannello [◀] [criterio] [▶]  [ASC/DESC] e la logica
/// di sort che lo accompagna.
/// </summary>
public class SeedSorter
{
    public SeedSortCriterion Criterion { get; set; } = SeedSortCriterion.Default;
    public bool Descending { get; set; } = true;
    public bool IncludeRarity { get; set; } = true;

    public Action OnChanged;

    private Rectangle prevBtnRect;
    private Rectangle nextBtnRect;
    private Rectangle dirBtnRect;
    private Rectangle labelRect;

    private bool prevHover;
    private bool nextHover;
    private bool dirHover;

    // Colori
    private static readonly Color bgColor       = new Color(62, 39, 25, 220);
    private static readonly Color bgHoverColor  = new Color(120, 82, 50, 240);
    private static readonly Color labelBgColor  = new Color(41, 26, 17, 220);
    private static readonly Color borderColor   = new Color(30, 20, 12, 255);
    private static readonly Color textColor     = new Color(240, 230, 210, 255);
    private static readonly Color accentColor   = new Color(255, 200, 100, 255);

    public List<SeedSortCriterion> GetAvailable()
    {
        var list = new List<SeedSortCriterion> { SeedSortCriterion.Default };
        if (IncludeRarity) list.Add(SeedSortCriterion.Rarita);
        list.Add(SeedSortCriterion.Vitalita);
        list.Add(SeedSortCriterion.Idratazione);
        list.Add(SeedSortCriterion.Metabolismo);
        list.Add(SeedSortCriterion.Vegetazione);
        list.Add(SeedSortCriterion.ResFreddo);
        list.Add(SeedSortCriterion.ResCaldo);
        list.Add(SeedSortCriterion.ResParassiti);
        list.Add(SeedSortCriterion.ResVuoto);
        return list;
    }

    public string GetCriterionLabel()
    {
        return Criterion switch
        {
            SeedSortCriterion.Default      => "Predefinito",
            SeedSortCriterion.Rarita       => "Rarita",
            SeedSortCriterion.Vitalita     => "Vitalita",
            SeedSortCriterion.Idratazione  => "Idratazione",
            SeedSortCriterion.Metabolismo  => "Metabolismo",
            SeedSortCriterion.Vegetazione  => "Vegetazione",
            SeedSortCriterion.ResFreddo    => "Res. Freddo",
            SeedSortCriterion.ResCaldo     => "Res. Caldo",
            SeedSortCriterion.ResParassiti => "Res. Parassiti",
            SeedSortCriterion.ResVuoto     => "Res. Vuoto",
            _ => "?"
        };
    }

    public List<Seed> Apply(IEnumerable<Seed> seeds)
    {
        if (seeds == null) return new List<Seed>();

        if (Criterion == SeedSortCriterion.Default)
            return seeds.ToList();

        Func<Seed, float> key = Criterion switch
        {
            SeedSortCriterion.Rarita       => s => (float)(int)s.rarity,
            SeedSortCriterion.Vitalita     => s => s.stats?.vitalita ?? 0f,
            SeedSortCriterion.Idratazione  => s => s.stats?.idratazione ?? 0f,
            SeedSortCriterion.Metabolismo  => s => s.stats?.metabolismo ?? 0f,
            SeedSortCriterion.Vegetazione  => s => s.stats?.vegetazione ?? 0f,
            SeedSortCriterion.ResFreddo    => s => s.stats?.resistenzaFreddo ?? 0f,
            SeedSortCriterion.ResCaldo     => s => s.stats?.resistenzaCaldo ?? 0f,
            SeedSortCriterion.ResParassiti => s => s.stats?.resistenzaParassiti ?? 0f,
            SeedSortCriterion.ResVuoto     => s => s.stats?.resistenzaVuoto ?? 0f,
            _ => _ => 0f
        };

        return Descending
            ? seeds.OrderByDescending(key).ToList()
            : seeds.OrderBy(key).ToList();
    }

    /// <summary>
    /// Controlla hover e click sui controlli. Chiamare da Update() prima di
    /// processare click sulla griglia. Ritorna true se uno stato e' cambiato.
    /// </summary>
    public bool HandleInput()
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        prevHover = HitRect(prevBtnRect, mx, my);
        nextHover = HitRect(nextBtnRect, mx, my);
        dirHover  = HitRect(dirBtnRect, mx, my);

        if (!Input.IsMouseButtonPressed(MouseButton.Left)) return false;

        var avail = GetAvailable();
        int idx = avail.IndexOf(Criterion);
        if (idx < 0) idx = 0;

        bool changed = false;

        if (prevHover)
        {
            idx = (idx - 1 + avail.Count) % avail.Count;
            Criterion = avail[idx];
            changed = true;
        }
        else if (nextHover || HitRect(labelRect, mx, my))
        {
            idx = (idx + 1) % avail.Count;
            Criterion = avail[idx];
            changed = true;
        }
        else if (dirHover)
        {
            Descending = !Descending;
            changed = true;
        }

        if (changed) OnChanged?.Invoke();
        return changed;
    }

    public bool IsMouseOverControls(int mx, int my)
    {
        return HitRect(prevBtnRect, mx, my)
            || HitRect(nextBtnRect, mx, my)
            || HitRect(dirBtnRect, mx, my)
            || HitRect(labelRect, mx, my);
    }

    private static bool HitRect(Rectangle r, int mx, int my)
    {
        return mx >= r.X && mx <= r.X + r.Width && my >= r.Y && my <= r.Y + r.Height;
    }

    public void Draw(int x, int y, int width, int height = 18)
    {
        int arrowW = 16;
        int dirW   = 28;
        int gap    = 3;

        int labelX = x + arrowW + gap;
        int labelW = width - (arrowW * 2) - dirW - (gap * 4);
        int rightArrowX = labelX + labelW + gap;
        int dirX = rightArrowX + arrowW + gap;

        prevBtnRect = new Rectangle(x, y, arrowW, height);
        labelRect   = new Rectangle(labelX, y, labelW, height);
        nextBtnRect = new Rectangle(rightArrowX, y, arrowW, height);
        dirBtnRect  = new Rectangle(dirX, y, dirW, height);

        // Freccia sinistra
        DrawButton(prevBtnRect, prevHover);
        DrawCenteredText("<", prevBtnRect, 10, textColor);

        // Etichetta criterio
        Graphics.DrawRectangleRounded(labelRect, 0.25f, 6, labelBgColor);
        Graphics.DrawRectangleRoundedLines(labelRect, 0.25f, 6, 1, borderColor);
        string label = GetCriterionLabel();
        DrawCenteredText(label, labelRect, 9, textColor);

        // Freccia destra
        DrawButton(nextBtnRect, nextHover);
        DrawCenteredText(">", nextBtnRect, 10, textColor);

        // Direzione
        DrawButton(dirBtnRect, dirHover);
        string dirLabel = Descending ? "DESC" : "ASC";
        DrawCenteredText(dirLabel, dirBtnRect, 9, accentColor);
    }

    private void DrawButton(Rectangle r, bool hover)
    {
        Graphics.DrawRectangleRounded(r, 0.3f, 6, hover ? bgHoverColor : bgColor);
        Graphics.DrawRectangleRoundedLines(r, 0.3f, 6, 1, borderColor);
    }

    private void DrawCenteredText(string text, Rectangle r, int fontSize, Color color)
    {
        int textW = text.Length * (fontSize / 2 + 1);
        int tx = (int)(r.X + (r.Width - textW) / 2);
        int ty = (int)(r.Y + (r.Height - fontSize) / 2);
        Graphics.DrawText(text, tx, ty, fontSize, color);
    }
}
