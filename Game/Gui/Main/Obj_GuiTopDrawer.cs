using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

public class Obj_GuiTopDrawer : GameElement
{
    private class DrawerButton
    {
        public string Label;
        public Sprite IconOff;
        public Sprite IconOn;
        public bool IsToggle;
        public Func<bool> GetActive;
        public Action<bool> OnToggle;
        public Action OnClick;
        public bool ShowMailBadge;
    }

    private const int ClosedTabHeight = 7;     // freccetta visibile a riposo
    private const int OpenHeight = 50;         // altezza extra quando aperto
    private const int ButtonSize = 28;
    private const int ButtonGap = 6;
    private const int SidePad = 8;

    private float openProgress = 0f;
    private const float AnimSpeed = 10f;
    private bool wasPressed = false;
    private int hoveredButtonIdx = -1;

    private List<DrawerButton> currentButtons = new();
    private uint lastRoomId = uint.MaxValue;

    private int TopY => GameProperties.TopBarHeight;

    public Obj_GuiTopDrawer()
    {
        this.guiLayer = true;
        this.depth = -800;
        this.persistent = true;
    }

    private int RawMouseY => Input.GetMouseY() + GameProperties.TopBarHeight;
    private int RawMouseX => Input.GetMouseX();

    private void RebuildIfRoomChanged()
    {
        uint rid = Room.GetActiveId();
        if (rid == lastRoomId) return;
        lastRoomId = rid;
        currentButtons.Clear();

        if (Game.room_main != null && rid == Game.room_main.id)
        {
            currentButtons.Add(new DrawerButton {
                Label = "Annaffiatoio",
                IconOff = AssetLoader.spriteWateringOff,
                IconOn = AssetLoader.spriteWateringOn,
                IsToggle = true,
                GetActive = () => Game.controller != null && Game.controller.annaffiatoioAttivo,
                OnToggle = (v) => { if (Game.controller != null) Game.controller.annaffiatoioAttivo = v; }
            });
            currentButtons.Add(new DrawerButton {
                Label = "Recupero Seme",
                IconOff = AssetLoader.spriteSeed1,
                IconOn = AssetLoader.spriteSeed1,
                OnClick = () => {
                    if (!SeedRecoverySystem.IsRecovering && !SeedRecoverySystem.IsConfirming
                        && !Game.IsModalitaPiantaggio && Game.guiSeedRecovery != null)
                        Game.guiSeedRecovery.ShowConfirmation();
                }
            });
        }

        // Posta sempre come ultimo bottone in ogni room
        currentButtons.Add(new DrawerButton {
            Label = "Posta",
            IconOff = AssetLoader.spriteSeed2,
            IconOn = AssetLoader.spriteSeed2,
            ShowMailBadge = true,
            OnClick = () => {
                if (Game.guiPostaPopup != null && !Game.guiPostaPopup.IsVisible)
                    Game.guiPostaPopup.Show();
            }
        });
    }

    private bool InteractionBlocked()
    {
        if (Game.guiMorte != null && Game.guiMorte.active) return true;
        if (Game.IsModalitaPiantaggio && Game.guiPiantaggio != null && Game.guiPiantaggio.isFalling) return true;
        if (SeedRecoverySystem.IsRewinding || SeedRecoverySystem.IsConfirming) return true;
        return false;
    }

    public override void Update()
    {
        RebuildIfRoomChanged();

        int sw = Rendering.camera.screenWidth;
        int currentBottom = TopY + ClosedTabHeight + (int)(OpenHeight * openProgress);

        bool blocked = InteractionBlocked();
        int mx = RawMouseX;
        int my = RawMouseY;

        // Apre anche con mouse sulla TopBar (y >= 0)
        bool hoverArea = !blocked && mx >= 0 && mx < sw && my >= 0 && my <= currentBottom + 4;
        float target = hoverArea ? 1f : 0f;
        openProgress += (target - openProgress) * Time.GetFrameTime() * AnimSpeed;
        openProgress = Math.Clamp(openProgress, 0f, 1f);

        // Consuma input quando aperto/hoverato così i bottoni sotto non scattano
        if (hoverArea || openProgress > 0.05f)
            InputGate.ConsumeMouse();

        hoveredButtonIdx = -1;
        if (openProgress > 0.5f && !blocked)
        {
            var (positions, btnY) = LayoutButtons(sw);
            for (int i = 0; i < currentButtons.Count; i++)
            {
                int bx = positions[i];
                if (mx >= bx && mx < bx + ButtonSize && my >= btnY && my < btnY + ButtonSize)
                {
                    hoveredButtonIdx = i;
                    break;
                }
            }
        }

        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);
        if (hoveredButtonIdx != -1 && wasPressed && !isPressed)
        {
            var b = currentButtons[hoveredButtonIdx];
            if (b.IsToggle)
            {
                bool now = !(b.GetActive?.Invoke() ?? false);
                b.OnToggle?.Invoke(now);
            }
            else
            {
                b.OnClick?.Invoke();
            }
        }
        wasPressed = isPressed && hoveredButtonIdx != -1;
    }

    private (int[] positions, int btnY) LayoutButtons(int sw)
    {
        int n = currentButtons.Count;
        int totalW = n * ButtonSize + Math.Max(0, n - 1) * ButtonGap;
        int startX = (sw - totalW) / 2;
        int[] pos = new int[n];
        for (int i = 0; i < n; i++) pos[i] = startX + i * (ButtonSize + ButtonGap);
        // Bottoni ancorati al fondo del cassetto (così scendono insieme all'animazione)
        int panelBottom = TopY + ClosedTabHeight + (int)(OpenHeight * openProgress);
        int btnY = panelBottom - ButtonSize - 8;
        return (pos, btnY);
    }

    public override void Draw()
    {
        int sw = Rendering.camera.screenWidth;
        int panelTop = TopY;
        int panelBottom = TopY + ClosedTabHeight + (int)(OpenHeight * openProgress);
        int panelH = panelBottom - panelTop;

        // Pannello cassetto
        Color bg = GuiTheme.NavBarBg;
        Color border = GuiTheme.NavBarOutline;

        int panelW = Math.Min(sw - 20, 280);
        int panelX = (sw - panelW) / 2;

        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelTop - 4, panelW, panelH + 4),
            0.25f, 8, bg
        );
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelTop - 4, panelW, panelH + 4),
            0.25f, 8, 1, border
        );

        // Bottoni quando il cassetto è almeno parzialmente aperto
        if (openProgress > 0.05f)
        {
            var (positions, btnY) = LayoutButtons(sw);
            byte alpha = (byte)(255 * Math.Clamp(openProgress * 1.4f, 0f, 1f));
            for (int i = 0; i < currentButtons.Count; i++)
            {
                DrawButton(currentButtons[i], positions[i], btnY, hoveredButtonIdx == i, alpha);
            }
        }

        // Freccetta indicatore: triangolo verso il basso quando chiuso, verso l'alto quando aperto
        DrawArrow(sw / 2, panelBottom - 4, openProgress);
    }

    private void DrawButton(DrawerButton b, int x, int y, bool hovered, byte alpha)
    {
        bool active = b.IsToggle && (b.GetActive?.Invoke() ?? false);

        Color fill = hovered ? new Color(110, 80, 140, alpha)
                   : (active ? new Color(90, 160, 110, alpha)
                             : new Color(60, 40, 80, alpha));
        Color brd = active ? new Color(141, 232, 91, alpha) : new Color(20, 12, 28, alpha);

        Graphics.DrawRectangleRounded(new Rectangle(x, y, ButtonSize, ButtonSize), 0.25f, 6, fill);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, y, ButtonSize, ButtonSize), 0.25f, 6, 1, brd);

        Sprite icon = active ? b.IconOn : b.IconOff;
        if (icon != null && icon.texture.Width > 0)
        {
            float scale = (ButtonSize - 10) / (float)icon.texture.Width;
            Vector2 pos = new Vector2(x + ButtonSize / 2, y + ButtonSize / 2);
            GameFunctions.DrawSprite(icon, pos, 0, scale);
        }

        if (b.ShowMailBadge)
        {
            int count = MailSystem.UnreadCount;
            if (count > 0)
            {
                string text = count > 9 ? "9+" : count.ToString();
                int badgeSize = 10;
                int bx = x + ButtonSize - badgeSize + 1;
                int by = y - 2;
                Graphics.DrawCircle(bx + badgeSize / 2, by + badgeSize / 2, badgeSize / 2f + 0.8f, new Color(255, 220, 220, alpha));
                Graphics.DrawCircle(bx + badgeSize / 2, by + badgeSize / 2, badgeSize / 2f, new Color(210, 50, 50, alpha));
                int textW = text.Length * 5;
                GuiTheme.DrawText(text, bx + (badgeSize - textW) / 2 + 1, by + 2, 8, new Color(255, 255, 255, alpha));
            }
        }
    }

    private void DrawArrow(int cx, int cy, float progress)
    {
        Color c = new Color(220, 210, 230, 255);
        // Mix tra ▼ (chiuso) e ▲ (aperto): rovescia in base a progress
        int dir = progress > 0.5f ? -1 : 1;
        int half = 3;
        for (int i = 0; i < 3; i++)
        {
            int yy = cy + i * dir;
            int w = (3 - i);
            Graphics.DrawRectangle(cx - w, yy, w * 2, 1, c);
        }
    }
}
