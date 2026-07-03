using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public enum TextAlign { Left, Center, Right }

public static class Hud
{
    // ── Click Guard ───────────────────────────────────────────────
    // Prevents multiple clickable Hud elements from firing in the same frame.
    // Reset each frame via BeginFrame() called from Rendering.

    private static bool _clickConsumed = false;

    public static bool ClickConsumed => _clickConsumed;

    public static void BeginFrame()
    {
        _clickConsumed = false;
    }

    // ── Panel ─────────────────────────────────────────────────────

    public struct PanelOptions
    {
        public Color? Bg;
        public Color? Border;
        public float? Roundness;
        public int? BorderThickness;
        public byte? Alpha;
    }

    public static void Panel(int x, int y, int w, int h, PanelOptions opt = default)
    {
        var bg = opt.Bg ?? GuiTheme.PanelBg;
        if (opt.Alpha.HasValue)
            bg = new Color(bg.R, bg.G, bg.B, opt.Alpha.Value);

        float round = opt.Roundness ?? 0.1f;
        int border = opt.BorderThickness ?? 2;

        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), round, 8, bg);

        if (border > 0)
        {
            var borderColor = opt.Border ?? GuiTheme.PanelOutline;
            Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), round, 8, border, borderColor);
        }
    }

    // ── Button ────────────────────────────────────────────────────

    public struct ButtonOptions
    {
        public Color? Bg;
        public Color? HoverBg;
        public Color? PressedBg;
        public Color? Border;
        public Color? HoverBorder;
        public Color? TextColor;
        public Color? DisabledBg;
        public Color? DisabledTextColor;
        public float? Roundness;
        public int? BorderThickness;
        public int? FontSize;
        public bool Disabled;
        public Sprite Icon;
        public byte? Alpha;
    }

    public static bool Button(int x, int y, int w, int h, string text, Action onClick = null, ButtonOptions opt = default)
    {
        bool hovered = !opt.Disabled && !_clickConsumed && MouseInside(x, y, w, h);
        bool pressed = hovered && Input.IsMouseButtonDown(MouseButton.Left);
        bool clicked = hovered && !_clickConsumed && Input.IsMouseButtonPressed(MouseButton.Left);

        float round = opt.Roundness ?? 0.3f;
        int border = opt.BorderThickness ?? 2;
        int fontSize = opt.FontSize ?? GuiTheme.FontSize;

        Color bg;
        Color textColor;

        if (opt.Disabled)
        {
            bg = opt.DisabledBg ?? new Color(80, 70, 60, 255);
            textColor = opt.DisabledTextColor ?? new Color(120, 120, 120, 255);
        }
        else if (pressed)
        {
            bg = opt.PressedBg ?? Darken(opt.Bg ?? new Color(101, 67, 43, 255), 20);
            textColor = opt.TextColor ?? GuiTheme.PanelText;
        }
        else if (hovered)
        {
            bg = opt.HoverBg ?? Lighten(opt.Bg ?? new Color(101, 67, 43, 255), 30);
            textColor = opt.TextColor ?? GuiTheme.PanelText;
        }
        else
        {
            bg = opt.Bg ?? new Color(101, 67, 43, 255);
            textColor = opt.TextColor ?? GuiTheme.PanelText;
        }

        if (opt.Alpha.HasValue)
            bg = new Color(bg.R, bg.G, bg.B, opt.Alpha.Value);

        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), round, 8, bg);

        if (border > 0)
        {
            var borderColor = (hovered ? opt.HoverBorder : null) ?? opt.Border;
            if (borderColor.HasValue)
            {
                var bc = borderColor.Value;
                if (opt.Alpha.HasValue)
                    bc = new Color(bc.R, bc.G, bc.B, opt.Alpha.Value);
                Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, w, h), round, 8, border, bc);
            }
        }

        int textW = GuiTheme.MeasureText(text ?? "", fontSize);
        int contentW = textW;
        int iconSize = 0;
        int iconGap = 0;

        if (opt.Icon != null && opt.Icon.texture.Width > 0)
        {
            iconSize = h - 8;
            iconGap = 4;
            contentW += iconSize + iconGap;
        }

        int contentX = x + (w - contentW) / 2;
        int textY = y + (h - fontSize) / 2;

        if (opt.Icon != null && opt.Icon.texture.Width > 0)
        {
            float scale = iconSize / (float)opt.Icon.texture.Width;
            Vector2 iconPos = new Vector2(contentX + iconSize / 2, y + h / 2);
            GameFunctions.DrawSprite(opt.Icon, iconPos, 0, scale, textColor);
            contentX += iconSize + iconGap;
        }

        if (!string.IsNullOrEmpty(text))
            GuiTheme.DrawText(text, contentX, textY, fontSize, textColor);

        if (clicked)
        {
            _clickConsumed = true;
            onClick?.Invoke();
        }

        return clicked;
    }

    // ── Label ─────────────────────────────────────────────────────

    public struct LabelOptions
    {
        public Color? Color;
        public int? FontSize;
        public TextAlign Align;
    }

    public static void Label(int x, int y, string text, LabelOptions opt = default)
    {
        var color = opt.Color ?? GuiTheme.PanelText;
        int fontSize = opt.FontSize ?? GuiTheme.FontSize;
        GuiTheme.DrawText(text, x, y, fontSize, color);
    }

    public static void Label(int x, int y, int w, string text, LabelOptions opt = default)
    {
        var color = opt.Color ?? GuiTheme.PanelText;
        int fontSize = opt.FontSize ?? GuiTheme.FontSize;
        int textW = GuiTheme.MeasureText(text, fontSize);

        int tx = opt.Align switch
        {
            TextAlign.Center => x + (w - textW) / 2,
            TextAlign.Right => x + w - textW,
            _ => x
        };

        GuiTheme.DrawText(text, tx, y, fontSize, color);
    }

    public static int LabelWrapped(int x, int y, int maxWidth, string text, LabelOptions opt = default)
    {
        var color = opt.Color ?? GuiTheme.PanelText;
        int fontSize = opt.FontSize ?? GuiTheme.FontSize;
        int lineH = fontSize + 3;

        string[] words = text.Split(' ');
        string line = "";
        int lineY = y;

        foreach (var word in words)
        {
            string test = line.Length > 0 ? line + " " + word : word;
            if (GuiTheme.MeasureText(test, fontSize) > maxWidth && line.Length > 0)
            {
                GuiTheme.DrawText(line, x, lineY, fontSize, color);
                lineY += lineH;
                line = word;
            }
            else
            {
                line = test;
            }
        }

        if (line.Length > 0)
        {
            GuiTheme.DrawText(line, x, lineY, fontSize, color);
            lineY += lineH;
        }

        return lineY - y;
    }

    // ── Icon ──────────────────────────────────────────────────────

    public struct IconOptions
    {
        public float? Scale;
        public Color? Tint;
        public float? Alpha;
        public bool Centered;
    }

    public static void Icon(int x, int y, Sprite sprite, IconOptions opt = default)
    {
        if (sprite == null || sprite.texture.Width <= 0) return;

        float scale = opt.Scale ?? 1.0f;
        float alpha = opt.Alpha ?? 1.0f;

        Vector2 pos;
        if (opt.Centered)
        {
            pos = new Vector2(x, y);
        }
        else
        {
            float halfW = sprite.texture.Width * scale / 2f;
            float halfH = sprite.texture.Height * scale / 2f;
            pos = new Vector2(x + halfW, y + halfH);
        }

        GameFunctions.DrawSprite(sprite, pos, 0, scale, opt.Tint, alpha);
    }

    // ── Tooltip ───────────────────────────────────────────────────

    public struct TooltipOptions
    {
        public Color? Bg;
        public Color? Border;
        public int? Padding;
        public int? FontSize;
        public Color? TextColor;
        public float? Roundness;
    }

    public static void Tooltip(int x, int y, string[] lines, TooltipOptions opt = default)
    {
        var textColor = opt.TextColor ?? new Color(220, 220, 230, 255);
        var colored = new List<(string text, Color color)>(lines.Length);
        foreach (var line in lines)
            colored.Add((line, textColor));
        Tooltip(x, y, colored, opt);
    }

    public static void Tooltip(int x, int y, List<(string text, Color color)> lines, TooltipOptions opt = default)
    {
        if (lines == null || lines.Count == 0) return;

        int fontSize = opt.FontSize ?? 10;
        int pad = opt.Padding ?? 8;
        int lineH = fontSize + 3;
        float round = opt.Roundness ?? 0.15f;

        int maxW = 0;
        foreach (var (text, _) in lines)
        {
            int w = GuiTheme.MeasureText(text, fontSize);
            if (w > maxW) maxW = w;
        }

        int tw = maxW + pad * 2;
        int th = pad * 2 + lines.Count * lineH;

        int sw = Rendering.camera.screenWidth;
        int sh = Rendering.camera.screenHeight;
        if (x + tw > sw - 2) x = sw - tw - 2;
        if (x < 2) x = 2;
        if (y + th > sh - 2) y = sh - th - 2;
        if (y < 2) y = 2;

        var bg = opt.Bg ?? new Color(30, 18, 10, 245);
        var border = opt.Border ?? new Color(62, 39, 25, 255);

        Graphics.DrawRectangleRounded(new Rectangle(x - 2, y - 2, tw + 4, th + 4), round, 6, border);
        Graphics.DrawRectangleRounded(new Rectangle(x, y, tw, th), round, 6, bg);

        int ly = y + pad;
        foreach (var (text, color) in lines)
        {
            GuiTheme.DrawText(text, x + pad, ly, fontSize, color);
            ly += lineH;
        }
    }

    // ── Progress Bar ──────────────────────────────────────────────

    public struct BarOptions
    {
        public Color? FillColor;
        public Color? TrackColor;
        public Color? BorderColor;
        public float? Roundness;
        public int? BorderThickness;
    }

    public static void Bar(int x, int y, int w, int h, float ratio, BarOptions opt = default)
    {
        float round = opt.Roundness ?? 0.5f;
        var track = opt.TrackColor ?? GuiTheme.BarTrack;
        var fill = opt.FillColor ?? GuiTheme.StatSalute;
        int border = opt.BorderThickness ?? 0;

        if (border > 0)
        {
            var borderColor = opt.BorderColor ?? GuiTheme.PanelOutline;
            Graphics.DrawRectangleRounded(new Rectangle(x - border, y - border, w + border * 2, h + border * 2), round, 4, borderColor);
        }

        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), round, 4, track);

        float clamped = Math.Clamp(ratio, 0f, 1f);
        int fillW = Math.Max(1, (int)(w * clamped));
        if (clamped > 0)
            Graphics.DrawRectangleRounded(new Rectangle(x, y, fillW, h), round, 4, fill);
    }

    // ── Toggle ────────────────────────────────────────────────────

    public struct ToggleOptions
    {
        public Color? OnColor;
        public Color? OffColor;
        public Color? KnobColor;
        public float? Roundness;
    }

    public static bool Toggle(int x, int y, int w, int h, bool on, Action onClick = null, ToggleOptions opt = default)
    {
        bool hovered = !_clickConsumed && MouseInside(x, y, w, h);
        bool clicked = hovered && !_clickConsumed && Input.IsMouseButtonPressed(MouseButton.Left);

        float round = opt.Roundness ?? 1.0f;
        var onColor = opt.OnColor ?? new Color(90, 160, 110, 255);
        var offColor = opt.OffColor ?? new Color(70, 48, 32, 255);
        var knobColor = opt.KnobColor ?? new Color(230, 230, 235, 255);

        Color bg = on ? onColor : offColor;
        if (hovered)
            bg = Lighten(bg, 25);

        Graphics.DrawRectangleRounded(new Rectangle(x, y, w, h), round, 8, bg);

        int knobSize = h - 4;
        int knobX = on ? x + w - knobSize - 2 : x + 2;
        int knobY = y + 2;
        Graphics.DrawRectangleRounded(new Rectangle(knobX, knobY, knobSize, knobSize), round, 8, knobColor);

        if (clicked)
        {
            _clickConsumed = true;
            onClick?.Invoke();
        }

        return clicked;
    }

    // ── Divider ───────────────────────────────────────────────────

    public static void Divider(int x, int y, int w, Color? color = null)
    {
        Graphics.DrawRectangle(x, y, w, 1, color ?? GuiTheme.PanelDivider);
    }

    // ── Overlay ───────────────────────────────────────────────────

    public static void Overlay(Color? color = null)
    {
        var c = color ?? new Color(0, 0, 0, 180);
        Graphics.DrawRectangle(0, 0, Rendering.camera.screenWidth, Rendering.camera.screenHeight, c);
    }

    // ── Hit testing ───────────────────────────────────────────────

    public static bool HitTest(int rx, int ry, int rw, int rh) => MouseInside(rx, ry, rw, rh);

    public static bool HitTest(Rectangle r) => MouseInside((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);

    public static bool Clicked(int rx, int ry, int rw, int rh)
    {
        if (_clickConsumed) return false;
        bool hit = Input.IsMouseButtonPressed(MouseButton.Left) && MouseInside(rx, ry, rw, rh);
        if (hit) _clickConsumed = true;
        return hit;
    }

    public static bool Clicked(Rectangle r)
    {
        if (_clickConsumed) return false;
        bool hit = Input.IsMouseButtonPressed(MouseButton.Left) && MouseInside((int)r.X, (int)r.Y, (int)r.Width, (int)r.Height);
        if (hit) _clickConsumed = true;
        return hit;
    }

    // ── Color helpers ─────────────────────────────────────────────

    public static Color WithAlpha(Color c, byte a) => new Color(c.R, c.G, c.B, a);

    public static Color Lighten(Color c, int amount = 25) => new Color((byte)Math.Min(255, c.R + amount), (byte)Math.Min(255, c.G + amount), (byte)Math.Min(255, c.B + amount), c.A);

    public static Color Darken(Color c, int amount = 25) => new Color((byte)Math.Max(0, c.R - amount), (byte)Math.Max(0, c.G - amount), (byte)Math.Max(0, c.B - amount), c.A);

    // ── Internal ──────────────────────────────────────────────────

    private static bool MouseInside(int rx, int ry, int rw, int rh)
    {
        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        return mx >= rx && mx < rx + rw && my >= ry && my < ry + rh;
    }
}
