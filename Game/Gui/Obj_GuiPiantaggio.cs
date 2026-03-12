using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Transformations;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace Plants;

/// <summary>
/// Schermata di selezione del seme da piantare.
/// Appare quando non c'e' un salvataggio o dopo la morte della pianta.
/// </summary>
public class Obj_GuiPiantaggio : GameElement
{
    private int cellSize = 50;
    private int spacing = 9;
    private int startX = 25;
    private int startY = 80;
    private int scrollY = 0;

    private int selectedIndex = -1;
    private int hoveredIndex = -1;
    private List<Seed> seeds = new();
    private List<Obj_Seed> visualSeeds = new();

    // Bottone "Pianta"
    private bool buttonHovered = false;

    // Colori
    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color cellSelectedColor = new Color(80, 160, 80, 250);
    private Color borderColor = new Color(62, 39, 25, 255);
    private Color borderSelectedColor = new Color(100, 220, 100, 255);
    private Color innerShadow = new Color(41, 26, 17, 180);

    private Color panelBg = new Color(25, 30, 20, 240);
    private Color panelBorder = new Color(80, 160, 80, 255);
    private Color verdeChiaro = new Color(100, 220, 100, 255);
    private Color bianco = new Color(240, 240, 240, 255);
    private Color grigio = new Color(160, 160, 160, 255);
    private Color buttonColor = new Color(60, 140, 60, 255);
    private Color buttonHoverColor = new Color(80, 180, 80, 255);
    private Color buttonDisabledColor = new Color(80, 80, 80, 200);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiPiantaggio()
    {
        this.guiLayer = true;
        this.depth = -2000;
        this.persistent = true;
        // roomId speciale per non essere toccato da SetActiveRoom
        this.roomId = uint.MaxValue;
    }

    public void Mostra()
    {
        this.active = true;
        selectedIndex = -1;
        hoveredIndex = -1;
        scrollY = 0;
        Aggiorna();
    }

    public void Nascondi()
    {
        this.active = false;
        DestroyVisualSeeds();
    }

    private void DestroyVisualSeeds()
    {
        foreach (var s in visualSeeds)
            s.Destroy();
        visualSeeds.Clear();
    }

    public void Aggiorna()
    {
        DestroyVisualSeeds();
        seeds = Inventario.get().GetAllSeeds();

        int columns = GetColumns();

        for (int i = 0; i < seeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;

            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Obj_Seed seedVisual = new Obj_Seed();
            seedVisual.scale = 1.8f;
            seedVisual.depth = -2001;
            seedVisual.guiLayer = true;
            seedVisual.persistent = true;
            seedVisual.roomId = uint.MaxValue;
            seedVisual.active = true;
            seedVisual.position = new Vector2(x + (cellSize / 2), y + (cellSize / 2));

            seedVisual.dati = seeds[i];
            seedVisual.color = seeds[i].color;
            visualSeeds.Add(seedVisual);
        }
    }

    private int GetColumns()
    {
        return Math.Max(1, (sw - startX * 2) / (cellSize + spacing));
    }

    public override void Update()
    {
        // Se siamo in modalita piantaggio ma in un'altra room, nasconditi silenziosamente
        if (Game.IsModalitaPiantaggio && active && Room.GetActiveId() != Game.room_main.id)
        {
            active = false;
            return;
        }

        if (!active) return;

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();
        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left);

        // Se non ci sono semi, controlla click su "Apri Pacchetti"
        if (seeds.Count == 0)
        {
            int cbtnW = 140;
            int cbtnH = 28;
            int cbtnX = (sw - cbtnW) / 2;
            int cbtnY = sh / 2 + 15;

            if (clicked && mx >= cbtnX && mx <= cbtnX + cbtnW && my >= cbtnY && my <= cbtnY + cbtnH)
            {
                Nascondi();
                Game.room_compost.SetActiveRoom();
            }
            return;
        }

        // Scroll
        float wheelDelta = Input.GetMouseWheelMove();
        if (wheelDelta != 0)
        {
            scrollY += (int)(wheelDelta * 20);

            int columns = GetColumns();
            int rows = (int)Math.Ceiling((float)seeds.Count / columns);
            int contentHeight = rows * (cellSize + spacing);
            int visibleHeight = sh - startY - 70;
            int minScroll = Math.Min(0, visibleHeight - contentHeight);
            scrollY = Math.Clamp(scrollY, minScroll, 0);

            UpdateVisualPositions();
        }

        hoveredIndex = -1;
        int columns2 = GetColumns();

        for (int i = 0; i < seeds.Count; i++)
        {
            int col = i % columns2;
            int row = i / columns2;

            int x = startX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize)
            {
                hoveredIndex = i;
                if (clicked)
                    selectedIndex = i;
                break;
            }
        }

        // Bottone "Pianta"
        if (selectedIndex >= 0 && selectedIndex < seeds.Count)
        {
            int btnW = 120;
            int btnH = 30;
            int btnX = (sw - btnW) / 2;
            int btnY = sh - 55;

            buttonHovered = mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH;

            if (buttonHovered && clicked)
            {
                Pianta(seeds[selectedIndex]);
            }
        }
        else
        {
            buttonHovered = false;
        }
    }

    private void UpdateVisualPositions()
    {
        int columns = GetColumns();
        for (int i = 0; i < visualSeeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            visualSeeds[i].position.Y = startY + row * (cellSize + spacing) + scrollY + (cellSize / 2);
        }
    }

    private void Pianta(Seed seed)
    {
        // Imposta il seme sulla pianta
        Game.pianta.SetSeed(seed.type);
        Game.pianta.seedBonus = seed.stats ?? SeedDataType.GetBonus(seed.type);
        Game.pianta.Reset();
        Game.pianta.SetNaturalColors(WorldManager.GetCurrentWorld());

        // Consuma il seme dall'inventario
        Inventario.get().RemoveSeed(seed);
        Inventario.get().Save();

        // Resetta lo stato del gioco
        Game.pianta.Stats.Salute = Game.pianta.proprieta.VitalitaMax;
        Game.pianta.Stats.Idratazione = 0.5f;
        Game.pianta.Stats.Ossigeno = 1.0f;
        Game.pianta.Stats.Metabolismo = 0.8f;
        Game.pianta.Stats.Temperatura = 20.0f;

        WaterSystem.Current = WaterSystem.Max;

        // Salva subito
        GameSave.get().data = new GameSaveData();
        GameSave.get().data.CurrentWorld = WorldManager.GetCurrentWorld();
        GameSave.get().Save();

        // Esci dalla modalita piantaggio
        Game.EsciModalitaPiantaggio();
    }

    public override void Draw()
    {
        if (!active) return;

        // Overlay sfondo scuro
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 200));

        // Titolo
        string titolo = "Scegli un Seme";
        int titoloW = titolo.Length * 9;
        Graphics.DrawText(titolo, (sw - titoloW) / 2, 15, 18, verdeChiaro);

        // Sottotitolo
        string sotto = seeds.Count > 0
            ? $"{seeds.Count} semi disponibili"
            : "Nessun seme disponibile!";
        int sottoW = sotto.Length * 5;
        Graphics.DrawText(sotto, (sw - sottoW) / 2, 40, 10, grigio);

        if (seeds.Count == 0)
        {
            string hint = "Apri dei pacchetti per ottenere semi";
            int hintW = hint.Length * 5;
            Graphics.DrawText(hint, (sw - hintW) / 2, sh / 2 - 10, 10, grigio);

            // Disegna bottone per andare al compost
            int cbtnW = 140;
            int cbtnH = 28;
            int cbtnX = (sw - cbtnW) / 2;
            int cbtnY = sh / 2 + 15;

            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();
            bool hoverCompost = mx >= cbtnX && mx <= cbtnX + cbtnW && my >= cbtnY && my <= cbtnY + cbtnH;

            Color cbtnColor = hoverCompost ? buttonHoverColor : buttonColor;
            Graphics.DrawRectangleRounded(new Rectangle(cbtnX, cbtnY, cbtnW, cbtnH), 0.3f, 8, cbtnColor);

            string cbtnText = "Apri Pacchetti";
            int cbtnTextW = cbtnText.Length * 6;
            Graphics.DrawText(cbtnText, cbtnX + (cbtnW - cbtnTextW) / 2, cbtnY + 7, 12, bianco);
            return;
        }

        // Griglia semi
        int columns = GetColumns();
        int rows = (int)Math.Ceiling((float)seeds.Count / columns);

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int i = row * columns + col;
                if (i >= seeds.Count) break;

                int x = startX + col * (cellSize + spacing);
                int y = startY + row * (cellSize + spacing) + scrollY;

                // Non disegnare se fuori schermo
                if (y + cellSize < startY - 10 || y > sh - 60) continue;

                Color bg = cellColor;
                Color border = borderColor;

                if (i == selectedIndex)
                {
                    bg = cellSelectedColor;
                    border = borderSelectedColor;
                }
                else if (i == hoveredIndex)
                {
                    bg = cellHoverColor;
                }

                Graphics.DrawRectangleRounded(
                    new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                    0.18f, 8, innerShadow);

                Graphics.DrawRectangleRounded(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, bg);

                Graphics.DrawRectangleRoundedLines(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, 3, border);
            }
        }

        // Disegna i semi visuali
        for (int i = 0; i < visualSeeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            int y = startY + row * (cellSize + spacing) + scrollY;
            if (y + cellSize < startY - 10 || y > sh - 60) continue;

            visualSeeds[i].Draw();
        }

        // Info seme selezionato + bottone
        if (selectedIndex >= 0 && selectedIndex < seeds.Count)
        {
            Seed sel = seeds[selectedIndex];

            // Nome del seme sotto la griglia
            string nome = SeedDataType.GetName(sel.type);
            int nomeW = nome.Length * 6;
            Graphics.DrawText(nome, (sw - nomeW) / 2, sh - 85, 12, bianco);

            // Bottone "Pianta"
            int btnW = 120;
            int btnH = 30;
            int btnX = (sw - btnW) / 2;
            int btnY = sh - 55;

            Color btnColor = buttonHovered ? buttonHoverColor : buttonColor;
            Graphics.DrawRectangleRounded(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, btnColor);
            Graphics.DrawRectangleRoundedLines(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, 2, verdeChiaro);

            string btnText = "Pianta!";
            int btnTextW = btnText.Length * 7;
            Graphics.DrawText(btnText, btnX + (btnW - btnTextW) / 2, btnY + 8, 14, bianco);
        }
    }
}
