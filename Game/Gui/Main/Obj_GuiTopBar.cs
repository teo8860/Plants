using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using Raylib_CSharp.Windowing;
using System.Numerics;

namespace Plants;

public class Obj_GuiTopBar : GameElement
{
    public const int BarHeight = GameProperties.TopBarHeight;

    private const int PadH = 4;
    private const int CurrencyGap = 16;
    private const int ButtonSize = 16;
    private const int ButtonGap = 3;

    private bool wasPressed = false;
    private int hoveredButton = -1; // 0 = X, 1 = Opzioni

    public Obj_GuiTopBar()
    {
        this.guiLayer = true;
        this.depth = -700;
        this.persistent = true;
    }

    private int RawMouseY => Input.GetMouseY() + GameProperties.TopBarHeight;
    private int RawMouseX => Input.GetMouseX();

    public override void Update()
    {
        // Disabilita interazione durante stati bloccanti
        if (Game.guiMorte != null && Game.guiMorte.active) { hoveredButton = -1; return; }
        if (Game.IsModalitaPiantaggio && Game.guiPiantaggio != null && Game.guiPiantaggio.isFalling) { hoveredButton = -1; return; }

        int sw = Rendering.camera.screenWidth;
        int my = RawMouseY;
        int mx = RawMouseX;

        hoveredButton = -1;

        // Consuma input quando il mouse è sopra la barra (y < TopBarHeight in coord raw)
        if (my >= 0 && my < BarHeight)
            InputGate.ConsumeMouse();

        // X (estrema destra)
        int xBtnX = sw - PadH - ButtonSize;
        int btnY = (BarHeight - ButtonSize) / 2;
        if (mx >= xBtnX && mx < xBtnX + ButtonSize && my >= btnY && my < btnY + ButtonSize)
            hoveredButton = 0;

        // Opzioni (a sinistra della X)
        int optBtnX = xBtnX - ButtonGap - ButtonSize;
        if (mx >= optBtnX && mx < optBtnX + ButtonSize && my >= btnY && my < btnY + ButtonSize)
            hoveredButton = 1;

        bool isPressed = Input.IsMouseButtonDown(MouseButton.Left);
        if (hoveredButton != -1 && wasPressed && !isPressed)
        {
            if (hoveredButton == 0)
            {
                // Chiudi finestra: nascondi nella tray (oppure esci se l'utente ha attivato CloseOnX)
                if (GameConfig.get().CloseOnX)
                    Program.ExitGame();
                else
                    Window.SetState(ConfigFlags.HiddenWindow);
            }
            else if (hoveredButton == 1)
            {
                if (Game.guiOpzioniPopup != null && !Game.guiOpzioniPopup.IsVisible)
                    Game.guiOpzioniPopup.Show();
            }
        }
        wasPressed = isPressed && hoveredButton != -1;
    }

    public override void Draw()
    {
        int sw = Rendering.camera.screenWidth;

        // Sfondo barra (riusa palette nav bar)
        Graphics.DrawRectangle(0, 0, sw, BarHeight, GuiTheme.NavBarBg);
        Graphics.DrawRectangle(0, BarHeight - 2, sw, 2, GuiTheme.NavBarOutline);

        // ── Currencies (sinistra) ──────────────────────────────────
        int foglie = 0, foglieOro = 0, essenza = SeedUpgradeSystem.Essence;
        if (Game.pianta != null)
        {
            foglie = Game.pianta.Stats.FoglieAccumulate;
            foglieOro = Game.pianta.Stats.FoglieOro;
        }

        int x = PadH;
        int y = (BarHeight - GuiTheme.FontSize) / 2;

        x = DrawCurrency(x, y, "F", new Color(141, 232, 91, 255), foglie);
        x += CurrencyGap;
        x = DrawCurrency(x, y, "O", new Color(255, 215, 80, 255), foglieOro);
        x += CurrencyGap;
        x = DrawCurrency(x, y, "E", new Color(180, 140, 232, 255), essenza);

        // ── Bottoni (destra): Opzioni + X ──────────────────────────
        int btnY = (BarHeight - ButtonSize) / 2;
        int xBtnX = sw - PadH - ButtonSize;
        int optBtnX = xBtnX - ButtonGap - ButtonSize;

        DrawIconButton(optBtnX, btnY, hoveredButton == 1, isClose: false);
        DrawIconButton(xBtnX, btnY, hoveredButton == 0, isClose: true);
    }

    private int DrawCurrency(int x, int y, string letter, Color iconColor, int amount)
    {
        string txt = amount.ToString("N0", System.Globalization.CultureInfo.InvariantCulture).Replace(',', '.');
        int txtW = GuiTheme.MeasureText(txt);

        int iconR = 5;
        int innerPadL = 3;
        int innerGap = 4;
        int innerPadR = 5;
        int boxW = innerPadL + iconR * 2 + innerGap + txtW + innerPadR;
        int boxH = BarHeight - 6;
        int boxY = (BarHeight - boxH) / 2;

        Color boxBg = new Color(20, 12, 28, 255);
        Color boxBorder = new Color(0, 0, 0, 200);
        Graphics.DrawRectangleRounded(new Rectangle(x, boxY, boxW, boxH), 0.45f, 6, boxBg);
        Graphics.DrawRectangleRoundedLines(new Rectangle(x, boxY, boxW, boxH), 0.45f, 6, 1, boxBorder);

        int cx = x + innerPadL + iconR;
        int cy = BarHeight / 2;
        Graphics.DrawCircleV(new Vector2(cx, cy), iconR, iconColor);
        GuiTheme.DrawText(letter, cx - 2, cy - GuiTheme.FontSize / 2, new Color(20, 12, 28, 255), 8);

        int textX = cx + iconR + innerGap;
        GuiTheme.DrawText(txt, textX, y, GuiTheme.PanelText);

        return x + boxW;
    }

    private void DrawIconButton(int x, int y, bool hovered, bool isClose)
    {
        Color bg = hovered ? GuiTheme.TabHoverBg : GuiTheme.TabInactiveBg;
        Graphics.DrawRectangle(x, y, ButtonSize, ButtonSize, bg);
        Graphics.DrawRectangle(x, y, ButtonSize, 1, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x, y + ButtonSize - 1, ButtonSize, 1, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x, y, 1, ButtonSize, GuiTheme.TabOutline);
        Graphics.DrawRectangle(x + ButtonSize - 1, y, 1, ButtonSize, GuiTheme.TabOutline);

        if (isClose)
        {
            // X: due diagonali
            Color c = GuiTheme.TabTextInactive;
            for (int i = 4; i < ButtonSize - 4; i++)
            {
                Graphics.DrawPixel(x + i, y + i, c);
                Graphics.DrawPixel(x + (ButtonSize - 1 - i), y + i, c);
            }
        }
        else
        {
            // Ingranaggio stilizzato: croce + cerchio centrale
            Color c = GuiTheme.TabTextInactive;
            int cx = x + ButtonSize / 2;
            int cy = y + ButtonSize / 2;
            Graphics.DrawRectangle(cx - 1, y + 3, 2, ButtonSize - 6, c);
            Graphics.DrawRectangle(x + 3, cy - 1, ButtonSize - 6, 2, c);
            Graphics.DrawCircleV(new Vector2(cx, cy), 3, c);
            Graphics.DrawCircleV(new Vector2(cx, cy), 1, bg);
        }
    }
}
