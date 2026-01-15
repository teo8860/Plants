using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;

namespace Plants;

public class GuiSeedDetailPanel : GameElement
{
    private bool isOpen = false;
    private float slideProgress = 0f;
    private float animationSpeed = 8f;

    private int panelWidth = 170;
    private int selectedSeedIndex = -1;

    // Colori stile legno
    private Color panelColor = new Color(82, 54, 35, 245);        // Marrone medio pannello
    private Color panelBorder = new Color(62, 39, 25, 255);       // Marrone scuro bordo
    private Color buttonColor = new Color(101, 67, 43, 255);      // Marrone medio bottone
    private Color buttonHoverColor = new Color(139, 90, 55, 255); // Marrone chiaro hover
    private Color buttonBorder = new Color(62, 39, 25, 255);      // Marrone scuro bordo bottone
    private Color textColor = new Color(245, 235, 220, 255);      // Beige/crema per testo

    private string[] buttonLabels = { "Unisci", "Scarta", "Migliora" };
    private int hoveredButton = -1;

    public Action<int, string> OnButtonClicked; // (seedIndex, buttonName)

    public GuiSeedDetailPanel() : base()
    {
        this.roomId = Game.inventoryRoom.id;
        this.guiLayer = true;
        this.depth = -100;
    }

    public void Open(int seedIndex)
    {
        selectedSeedIndex = seedIndex;
        isOpen = true;
    }

    public void Close()
    {
        isOpen = false;
        selectedSeedIndex = -1;
    }

    public void Toggle(int seedIndex)
    {
        if (isOpen && selectedSeedIndex == seedIndex)
            Close();
        else
            Open(seedIndex);
    }

    public bool IsOpen => isOpen;
    public float SlideProgress => slideProgress;
    public int PanelWidth => panelWidth;

    public override void Update()
    {
        float target = isOpen ? 1f : 0f;
        slideProgress += (target - slideProgress) * Time.GetFrameTime() * animationSpeed;
        slideProgress = Math.Clamp(slideProgress, 0f, 1f);

        if (slideProgress < 0.01f)
        {
            hoveredButton = -1;
            return;
        }

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;
        int panelX = screenWidth - (int)(panelWidth * slideProgress);

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        hoveredButton = -1;

        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int buttonsStartY = screenHeight - totalButtonsHeight - buttonMargin - 15;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + buttonMargin;
            int btnY = buttonsStartY + i * (buttonHeight + buttonSpacing);
            int btnWidth = panelWidth - buttonMargin * 2;

            if (mx >= btnX && mx <= btnX + btnWidth && my >= btnY && my <= btnY + buttonHeight)
            {
                hoveredButton = i;

                if (clicked)
                {
                    OnButtonClicked?.Invoke(selectedSeedIndex, buttonLabels[i]);
                }
                break;
            }
        }
    }

    public override void Draw()
    {
        if (slideProgress < 0.01f) return;

        int screenWidth = Rendering.camera.screenWidth;
        int screenHeight = Rendering.camera.screenHeight;
        int panelX = screenWidth - (int)(panelWidth * slideProgress);

        // Pannello principale
        Graphics.DrawRectangle(panelX, 0, panelWidth, screenHeight, panelColor);
        Graphics.DrawLine(panelX, 0, panelX, screenHeight, panelBorder);

        // Area stats (placeholder in alto)
        int statsAreaHeight = 150;
        Color statsBoxColor = new Color(62, 39, 25, 220); // Marrone scuro
        Color statsBoxBorder = new Color(41, 26, 17, 255); // Ombra ancora più scura
        
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX + 10, 15, panelWidth - 20, statsAreaHeight),
            0.1f,
            8,
            statsBoxColor
        );
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX + 10, 15, panelWidth - 20, statsAreaHeight),
            0.1f,
            8,
            2,
            statsBoxBorder
        );
        
        // Testo placeholder stats
        Graphics.DrawText("STATS", panelX + 18, 25, 14, textColor);
        if (selectedSeedIndex >= 0)
        {
            Color subtextColor = new Color(200, 180, 150, 255); // Beige più scuro
            Graphics.DrawText($"Seme #{selectedSeedIndex + 1}", panelX + 18, 50, 12, subtextColor);
        }

        // Bottoni in basso
        int buttonHeight = 36;
        int buttonSpacing = 16;
        int buttonMargin = 12;
        int totalButtonsHeight = buttonLabels.Length * buttonHeight + (buttonLabels.Length - 1) * buttonSpacing;
        int buttonsStartY = screenHeight - totalButtonsHeight - buttonMargin - 15;

        for (int i = 0; i < buttonLabels.Length; i++)
        {
            int btnX = panelX + buttonMargin;
            int btnY = buttonsStartY + i * (buttonHeight + buttonSpacing);
            int btnWidth = panelWidth - buttonMargin * 2;

            Color bg = (i == hoveredButton) ? buttonHoverColor : buttonColor;

            Graphics.DrawRectangleRounded(
                new Rectangle(btnX, btnY, btnWidth, buttonHeight),
                0.22f,
                8,
                bg
            );

            Graphics.DrawRectangleRoundedLines(
                new Rectangle(btnX, btnY, btnWidth, buttonHeight),
                0.22f,
                8,
                3,
                buttonBorder
            );

            int textWidth = buttonLabels[i].Length * 7;
            int textX = btnX + (btnWidth - textWidth) / 2;
            int textY = btnY + (buttonHeight - 14) / 2;
            Graphics.DrawText(buttonLabels[i], textX, textY, 14, textColor);
        }
    }
}
