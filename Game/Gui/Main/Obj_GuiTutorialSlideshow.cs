using System;
using System.Collections.Generic;
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Fonts;
using Raylib_CSharp.Transformations;

namespace Plants
{
    /// <summary>
    /// Una singola slide del tutorial.
    /// Icon e DrawExtra sono opzionali — usali per mostrare sprite o disegni custom nel pannello.
    /// </summary>
    public record TutorialSlide(
        string Titolo,
        string[] Righe,
        Sprite? Icon = null,
        Action<int, int>? DrawExtra = null  // (panelX, panelY)
    );

    public class Obj_GuiTutorialSlideshow : GameElement
    {
        // Layout
        private const int PanelW       = 260;
        private const int MinPanelH    = 165;
        private const int HeaderH      = 26;
        private const int FooterH      = 32;
        private const int BtnW         = 76;
        private const int BtnH         = 22;
        private const int IconAreaW    = 60;
        private const int IconSize     = 44;
        private const int LineH        = 15;
        private const int BodyFontSize = 11;
        private const int BodyPadding  = 10;

        // Colori
        private static readonly Color OverlayColor  = new Color(0,   0,   0,   140);
        private static readonly Color PanelBg       = new Color(28,  35,  22,  245);
        private static readonly Color PanelBorder   = new Color(100, 140, 80,  255);
        private static readonly Color HeaderBg      = new Color(55,  90,  40,  255);
        private static readonly Color TextColor     = new Color(220, 230, 200, 255);
        private static readonly Color SubTextColor  = new Color(170, 185, 150, 255);
        private static readonly Color BtnColor      = new Color(70,  115, 55,  255);
        private static readonly Color BtnHoverColor = new Color(90,  145, 70,  255);
        private static readonly Color BtnDisabled   = new Color(50,  65,  40,  255);
        private static readonly Color DotActive     = new Color(140, 200, 100, 255);
        private static readonly Color DotInactive   = new Color(70,  90,  55,  255);
        private static readonly Color IconBg        = new Color(40,  55,  30,  200);
        private static readonly Color IconBorder    = new Color(90,  130, 70,  255);

        // Stato
        private bool isVisible = false;
        private TutorialSlide[] slides = Array.Empty<TutorialSlide>();
        private int currentIndex = 0;
        private Action? onComplete;
        private Action? onSkip;

        // Hover
        private bool hoverNext = false;
        private bool hoverPrev = false;
        private bool hoverSkip = false;

        // Screen helpers
        private int sw => Rendering.camera.screenWidth;
        private int sh => Rendering.camera.screenHeight;
        private int panelX => (sw - PanelW) / 2;
        private int currentPanelH = MinPanelH;
        private int panelY => (sh - currentPanelH) / 2;

        public Obj_GuiTutorialSlideshow()
        {
            guiLayer = true;
            persistent = true;
            depth = -5000;
        }

        // ── API pubblica ────────────────────────────────────────────

        /// <summary>Mostra le slide. onDone viene chiamato quando il player clicca "Capito!" sull'ultima.</summary>
        public void Show(TutorialSlide[] slideList, Action? onDone = null, Action? onSkipTutorial = null)
        {
            slides = slideList;
            currentIndex = 0;
            onComplete = onDone;
            onSkip = onSkipTutorial;
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        public bool IsVisible => isVisible;

        // ── Update ─────────────────────────────────────────────────

        public override void Update()
        {
            if (!isVisible || slides.Length == 0) return;

            currentPanelH = ComputePanelH(slides[currentIndex]);

            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            hoverNext = Contains(NextBtnRect(), mx, my);
            hoverPrev = currentIndex > 0 && Contains(PrevBtnRect(), mx, my);
            hoverSkip = onSkip != null && Contains(SkipBtnRect(), mx, my);

            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                if (hoverSkip)
                {
                    isVisible = false;
                    onSkip?.Invoke();
                }
                else if (hoverNext)
                {
                    if (currentIndex < slides.Length - 1)
                        currentIndex++;
                    else
                        Complete();
                }
                else if (hoverPrev)
                {
                    currentIndex--;
                }
            }
        }

        // ── Draw ───────────────────────────────────────────────────

        public override void Draw()
        {
            if (!isVisible || slides.Length == 0) return;

            var slide = slides[currentIndex];
            currentPanelH = ComputePanelH(slide);
            int px = panelX;
            int py = panelY;

            Graphics.DrawRectangle(0, 0, sw, sh, OverlayColor);

            // Pannello
            Graphics.DrawRectangleRounded(new Rectangle(px, py, PanelW, currentPanelH), 0.08f, 8, PanelBg);
            Graphics.DrawRectangleRoundedLines(new Rectangle(px, py, PanelW, currentPanelH), 0.08f, 8, 2, PanelBorder);

            // Header
            Graphics.DrawRectangleRounded(new Rectangle(px, py, PanelW, HeaderH), 0.2f, 8, HeaderBg);
            int titleW = GuiTheme.MeasureText(slide.Titolo, 14);
            GuiTheme.DrawText(slide.Titolo, px + (PanelW - titleW) / 2, py + 7, 14, TextColor);

            // Corpo: layout a una o due colonne
            bool hasVisual = slide.Icon != null || slide.DrawExtra != null;
            if (hasVisual)
                DrawBodyTwoColumns(slide, px, py);
            else
                DrawBodyText(slide, px, py);

            DrawDots(px, py);
            DrawButtons(px, py, slide);
            if (onSkip != null) DrawSkipButton();
        }

        private static List<string> WrapLines(string[] lines, int maxWidth, int fontSize)
        {
            var result = new List<string>();
            foreach (var line in lines)
            {
                if (line.Length == 0)
                {
                    result.Add("");
                    continue;
                }
                int w = GuiTheme.MeasureText(line, fontSize);
                if (w <= maxWidth)
                {
                    result.Add(line);
                    continue;
                }
                var words = line.Split(' ');
                string current = "";
                foreach (var word in words)
                {
                    string test = current.Length == 0 ? word : current + " " + word;
                    if (GuiTheme.MeasureText(test, fontSize) > maxWidth && current.Length > 0)
                    {
                        result.Add(current);
                        current = word;
                    }
                    else
                    {
                        current = test;
                    }
                }
                if (current.Length > 0)
                    result.Add(current);
            }
            return result;
        }

        private int ComputePanelH(TutorialSlide slide)
        {
            bool hasVisual = slide.Icon != null || slide.DrawExtra != null;
            int maxW = hasVisual ? PanelW - IconAreaW - 20 : PanelW - 16;
            var wrapped = WrapLines(slide.Righe, maxW, BodyFontSize);
            int bodyH = wrapped.Count * LineH + BodyPadding * 2;
            return Math.Max(MinPanelH, HeaderH + bodyH + FooterH);
        }

        // Testo centrato — nessuna icona
        private void DrawBodyText(TutorialSlide slide, int px, int py)
        {
            int maxW = PanelW - 16;
            var wrapped = WrapLines(slide.Righe, maxW, BodyFontSize);
            int textY = py + HeaderH + BodyPadding;
            foreach (var riga in wrapped)
            {
                if (riga.Length > 0)
                {
                    int rigaW = GuiTheme.MeasureText(riga, BodyFontSize);
                    int clampedX = px + Math.Max(4, (PanelW - rigaW) / 2);
                    GuiTheme.DrawText(riga, clampedX, textY, BodyFontSize, SubTextColor);
                }
                textY += LineH;
            }
        }

        // Testo a sinistra + icona/custom a destra
        private void DrawBodyTwoColumns(TutorialSlide slide, int px, int py)
        {
            int bodyTop  = py + HeaderH + BodyPadding;
            int textColW = PanelW - IconAreaW - 20;
            int textX    = px + 8;

            var wrapped = WrapLines(slide.Righe, textColW, BodyFontSize);
            int textY = bodyTop;
            foreach (var riga in wrapped)
            {
                if (riga.Length > 0)
                    GuiTheme.DrawText(riga, textX, textY, BodyFontSize, SubTextColor);
                textY += LineH;
            }

            int iconColX  = px + PanelW - IconAreaW - 4;
            int iconBodyH = currentPanelH - HeaderH - FooterH;
            int iconAreaY = py + HeaderH + (iconBodyH - IconSize) / 2;

            // Sfondo icona
            Graphics.DrawRectangleRounded(
                new Rectangle(iconColX, iconAreaY, IconAreaW, IconSize), 0.15f, 8, IconBg);
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(iconColX, iconAreaY, IconAreaW, IconSize), 0.15f, 8, 1, IconBorder);

            if (slide.Icon != null)
            {
                float scale = (float)IconSize / Math.Max(slide.Icon.texture.Width, slide.Icon.texture.Height) * 0.85f;
                var center = new Vector2(iconColX + IconAreaW / 2f, iconAreaY + IconSize / 2f);
                GameFunctions.DrawSprite(slide.Icon, center, 0f, scale);
            }
            else
            {
                slide.DrawExtra?.Invoke(iconColX + 4, iconAreaY + 4);
            }
        }

        private void DrawDots(int px, int py)
        {
            if (slides.Length <= 1) return;

            const int dotSize = 6;
            const int dotGap  = 4;
            int totalW     = slides.Length * dotSize + (slides.Length - 1) * dotGap;
            int dotStartX  = px + (PanelW - totalW) / 2;
            int dotY       = py + currentPanelH - FooterH + 5;

            for (int i = 0; i < slides.Length; i++)
            {
                Color c = i == currentIndex ? DotActive : DotInactive;
                Graphics.DrawRectangleRounded(
                    new Rectangle(dotStartX + i * (dotSize + dotGap), dotY, dotSize, dotSize),
                    1f, 4, c);
            }
        }

        private void DrawButtons(int px, int py, TutorialSlide slide)
        {
            bool isLast = currentIndex == slides.Length - 1;
            string nextLabel = isLast ? "Capito!" : "Avanti >";

            // Bottone Avanti / Capito
            var rNext = NextBtnRect();
            Color nextCol = hoverNext ? BtnHoverColor : BtnColor;
            Graphics.DrawRectangleRounded(rNext, 0.3f, 8, nextCol);
            int nextW = GuiTheme.MeasureText(nextLabel, 11);
            GuiTheme.DrawText(nextLabel,
                (int)(rNext.X + (rNext.Width - nextW) / 2),
                (int)(rNext.Y + 5), 11, TextColor);

            // Bottone Indietro
            if (currentIndex > 0)
            {
                var rPrev = PrevBtnRect();
                Color prevCol = hoverPrev ? BtnHoverColor : BtnDisabled;
                Graphics.DrawRectangleRounded(rPrev, 0.3f, 8, prevCol);
                string prevLabel = "< Indietro";
                int prevW = GuiTheme.MeasureText(prevLabel, 11);
                GuiTheme.DrawText(prevLabel,
                    (int)(rPrev.X + (rPrev.Width - prevW) / 2),
                    (int)(rPrev.Y + 5), 11, TextColor);
            }
        }

        // ── Rects ──────────────────────────────────────────────────

        private Rectangle NextBtnRect() =>
            new Rectangle(panelX + PanelW - BtnW - 10, panelY + currentPanelH - BtnH - 6, BtnW, BtnH);

        private Rectangle PrevBtnRect()
        {
            if (onSkip != null)
                return new Rectangle(panelX + (PanelW - BtnW) / 2, panelY + currentPanelH - BtnH - 6, BtnW, BtnH);
            return new Rectangle(panelX + 10, panelY + currentPanelH - BtnH - 6, BtnW, BtnH);
        }

        private static readonly Color SkipBtnColor      = new Color(140, 60, 50, 255);
        private static readonly Color SkipBtnHoverColor = new Color(180, 75, 60, 255);

        private Rectangle SkipBtnRect() =>
            new Rectangle(panelX + 10, panelY + currentPanelH - BtnH - 6, BtnW, BtnH);

        private void DrawSkipButton()
        {
            var r = SkipBtnRect();
            Color bg = hoverSkip ? SkipBtnHoverColor : SkipBtnColor;
            Graphics.DrawRectangleRounded(r, 0.3f, 8, bg);

            string label = "Salta >>";
            int lw = GuiTheme.MeasureText(label, 11);
            GuiTheme.DrawText(label, (int)(r.X + (r.Width - lw) / 2), (int)(r.Y + 5), 11, TextColor);
        }

        private static bool Contains(Rectangle r, int mx, int my) =>
            mx >= r.X && mx <= r.X + r.Width && my >= r.Y && my <= r.Y + r.Height;

        private void Complete()
        {
            isVisible = false;
            onComplete?.Invoke();
        }
    }
}
