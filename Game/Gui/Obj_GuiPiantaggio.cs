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
    private int startY = 82;
    private int scrollY = 0;

    private int selectedIndex = -1;
    private int hoveredIndex = -1;
    private List<Seed> seeds = new();
    private List<Obj_Seed> visualSeeds = new();

    // Ordinamento
    private SeedSorter sorter = new SeedSorter { IncludeRarity = true };
    private Seed selectedSeedRef = null;

    // Bottone "Pianta"
    private bool buttonHovered = false;

    // Animazione caduta seme
    public bool isFalling = false;
    private float fallSemeY = 0f;
    private float fallSemeX = 0f;
    private float fallStartY = 0f;
    private float fallTargetY = 0f;
    private float fallSpeed = 0f;
    private Seed fallingSeed = null;
    private Obj_Seed fallingSeedVisual = null;
    private float fallOscillation = 0f;
    private bool fallImpacted = false;
    private float postImpactTimer = 0f;
    private float fallElapsed = 0f; // tempo dall'inizio animazione (per skip)
    private const float FALL_SCALE_START = 3.0f;
    private const float FALL_SCALE_END = 1.5f;
    private const float FALL_SKIP_DELAY = 0.3f;

    // Particelle
    private List<TutorialParticle> particles = new();

    // Pulse per animazioni
    private float pulse = 0f;

    // Colori
    private Color cellColor = new Color(101, 67, 43, 250);
    private Color cellHoverColor = new Color(139, 90, 55, 250);
    private Color cellSelectedColor = new Color(80, 160, 80, 250);
    private Color borderColor = new Color(62, 39, 25, 255);
    private Color innerShadow = new Color(41, 26, 17, 180);

    private Color verdeChiaro = new Color(100, 220, 100, 255);
    private Color bianco = new Color(240, 240, 240, 255);
    private Color grigio = new Color(160, 160, 160, 255);
    private Color grigioScuro = new Color(100, 100, 100, 255);
    private Color buttonColor = new Color(60, 140, 60, 255);
    private Color buttonHoverColor = new Color(80, 180, 80, 255);

    private int sw => Rendering.camera.screenWidth;
    private int sh => Rendering.camera.screenHeight;

    public Obj_GuiPiantaggio()
    {
        this.guiLayer = true;
        this.depth = -2000;
        this.persistent = true;
        this.roomId = uint.MaxValue;
    }

    public void Mostra()
    {
        this.active = true;
        selectedIndex = -1;
        selectedSeedRef = null;
        hoveredIndex = -1;
        scrollY = 0;
        isFalling = false;
        fallImpacted = false;
        particles.Clear();
        pulse = 0f;
        sorter.OnChanged = () => { scrollY = 0; Aggiorna(); };
        StarterSeedSystem.GrantIfNeeded();
        Aggiorna();

        // Disabilita elementi del gioco dietro al popup (SetActiveRoom li riattiva,
        // quindi serve rifarlo ogni volta che si torna al giardino in piantaggio)
        if (Game.pianta != null) Game.pianta.active = false;
        if (Game.statsPanel != null) Game.statsPanel.active = false;
        if (Game.toolbar != null) Game.toolbar.active = false;
        if (Game.toolbarBottom != null) Game.toolbarBottom.active = false;
        if (Game.innaffiatoio != null) Game.innaffiatoio.active = false;
    }

    public void Nascondi()
    {
        this.active = false;
        isFalling = false;
        DestroyVisualSeeds();
        DestroyFallingSeed();
        particles.Clear();
    }

    private void DestroyVisualSeeds()
    {
        foreach (var s in visualSeeds)
            s.Destroy();
        visualSeeds.Clear();
    }

    private void DestroyFallingSeed()
    {
        if (fallingSeedVisual != null)
        {
            fallingSeedVisual.Destroy();
            fallingSeedVisual = null;
        }
    }

    public void Aggiorna()
    {
        DestroyVisualSeeds();
        seeds = sorter.Apply(Inventario.get().GetAllSeeds());

        // Mantieni la selezione sul seme stesso anche dopo il sort
        if (selectedSeedRef != null)
        {
            int newIdx = seeds.IndexOf(selectedSeedRef);
            selectedIndex = newIdx;
            if (newIdx < 0) selectedSeedRef = null;
        }

        int columns = GetColumns();
        int gridW = columns * (cellSize + spacing) - spacing;
        int gridStartX = (sw - gridW) / 2;

        for (int i = 0; i < seeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;

            int x = gridStartX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing);

            Obj_Seed seedVisual = new Obj_Seed();
            seedVisual.scale = 1.8f;
            seedVisual.depth = -2001;
            seedVisual.guiLayer = true;
            seedVisual.persistent = true;
            seedVisual.roomId = uint.MaxValue;
            // active=false: il render loop automatico li ignora, vengono disegnati
            // manualmente da DrawGrid() con il corretto culling dello scroll
            seedVisual.active = false;
            seedVisual.position = new Vector2(x + (cellSize / 2), y + (cellSize / 2));

            seedVisual.dati = seeds[i];
            seedVisual.color = seeds[i].color;
            visualSeeds.Add(seedVisual);
        }
    }

    private int GetColumns()
    {
        int gridStartX = 15;
        return Math.Max(1, (sw - gridStartX * 2) / (cellSize + spacing));
    }

    private int GetGridStartX()
    {
        int columns = GetColumns();
        int gridW = columns * (cellSize + spacing) - spacing;
        return (sw - gridW) / 2;
    }

    public override void Update()
    {
        if (Game.IsModalitaPiantaggio && active && Room.GetActiveId() != Game.room_main.id)
        {
            active = false;
            DestroyVisualSeeds();
            DestroyFallingSeed();
            isFalling = false;
            particles.Clear();
            return;
        }

        if (!active) return;

        float deltaTime = Time.GetFrameTime();
        pulse += deltaTime * 3f;

        // Aggiorna particelle
        UpdateParticles(deltaTime);

        // Se stiamo animando la caduta, aggiorna solo quella
        if (isFalling)
        {
            UpdateFalling(deltaTime);
            return;
        }

        int mx = Input.GetMouseX();
        int my = Input.GetMouseY();

        // Sort controls: intercetta eventuali click prima della griglia
        bool sortConsumed = false;
        if (seeds.Count > 0 || sorter.Criterion != SeedSortCriterion.Default)
        {
            sortConsumed = sorter.HandleInput();
        }

        bool clicked = Input.IsMouseButtonPressed(MouseButton.Left) && !sortConsumed;

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
            int visibleHeight = sh - startY - 220;
            int minScroll = Math.Min(0, visibleHeight - contentHeight);
            scrollY = Math.Clamp(scrollY, minScroll, 0);

            UpdateVisualPositions();
        }

        hoveredIndex = -1;
        int cols = GetColumns();
        int gridStartX = GetGridStartX();

        bool overSort = sorter.IsMouseOverControls(mx, my);

        for (int i = 0; i < seeds.Count; i++)
        {
            int col = i % cols;
            int row = i / cols;

            int x = gridStartX + col * (cellSize + spacing);
            int y = startY + row * (cellSize + spacing) + scrollY;

            if (mx >= x && mx <= x + cellSize && my >= y && my <= y + cellSize && !overSort)
            {
                hoveredIndex = i;
                if (clicked)
                {
                    selectedIndex = i;
                    selectedSeedRef = seeds[i];
                }
                break;
            }
        }

        // Bottone "Pianta" (coordinate allineate con DrawSelectedInfo)
        if (selectedIndex >= 0 && selectedIndex < seeds.Count)
        {
            int panelH = 160;
            int panelY = sh - panelH - 5 - Obj_GuiBottomNavigation.BAR_HEIGHT;
            int btnW = 120;
            int btnH = 22;
            int btnX = (sw - btnW) / 2;
            int btnY = panelY + panelH - btnH - 4;

            buttonHovered = mx >= btnX && mx <= btnX + btnW && my >= btnY && my <= btnY + btnH;

            if (buttonHovered && clicked)
            {
                StartFallAnimation(seeds[selectedIndex]);
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
        int gridStartX = GetGridStartX();
        for (int i = 0; i < visualSeeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            visualSeeds[i].position.X = gridStartX + col * (cellSize + spacing) + (cellSize / 2);
            visualSeeds[i].position.Y = startY + row * (cellSize + spacing) + scrollY + (cellSize / 2);
        }
    }

    // --- Animazione caduta ---

    private void StartFallAnimation(Seed seed)
    {
        isFalling = true;
        fallImpacted = false;
        fallingSeed = seed;
        fallSemeX = sw / 2f;
        fallStartY = -20f;
        fallSemeY = fallStartY;
        // Punto di atterraggio piu' in basso, dove si trova il terreno visibile
        // dietro l'overlay (coerente con il tutorial che usa ~350 su schermo 500).
        fallTargetY = sh - 150f;
        fallSpeed = 0f;
        fallOscillation = 0f;
        postImpactTimer = 0f;
        fallElapsed = 0f;
        particles.Clear();

        // Nascondi la griglia visuale
        foreach (var s in visualSeeds)
            s.active = false;

        // Crea il seme visuale che cade
        fallingSeedVisual = new Obj_Seed();
        fallingSeedVisual.scale = FALL_SCALE_START;
        fallingSeedVisual.depth = -2002;
        fallingSeedVisual.guiLayer = true;
        fallingSeedVisual.persistent = true;
        fallingSeedVisual.roomId = uint.MaxValue;
        // active=false: evita il doppio disegno (render loop + DrawFalling)
        fallingSeedVisual.active = false;
        fallingSeedVisual.dati = seed;
        fallingSeedVisual.color = seed.color;
        fallingSeedVisual.position = new Vector2(fallSemeX, fallSemeY);
    }

    private void UpdateFalling(float deltaTime)
    {
        fallElapsed += deltaTime;

        // Skip con click dopo un breve delay iniziale: salta direttamente a Pianta
        if (fallElapsed > FALL_SKIP_DELAY && Input.IsMouseButtonPressed(MouseButton.Left))
        {
            DestroyFallingSeed();
            Pianta(fallingSeed);
            return;
        }

        if (!fallImpacted)
        {
            fallOscillation += deltaTime * 6f;
            float wobble = MathF.Sin(fallOscillation) * 3f;
            fallSemeX = sw / 2f + wobble;

            fallSpeed += 200f * deltaTime;
            fallSemeY += fallSpeed * deltaTime;

            if (fallSemeY >= fallTargetY)
            {
                fallSemeY = fallTargetY;
                fallImpacted = true;
                postImpactTimer = 0f;

                // Particelle impatto
                SpawnImpactParticles(fallSemeX, fallTargetY);
            }

            if (fallingSeedVisual != null)
            {
                fallingSeedVisual.position = new Vector2(fallSemeX, fallSemeY);
                // Shrink progressivo con la profondita' (prospettiva)
                float range = fallTargetY - fallStartY;
                float progress = range > 0f
                    ? Math.Clamp((fallSemeY - fallStartY) / range, 0f, 1f)
                    : 1f;
                fallingSeedVisual.scale = MathHelper.Lerp(FALL_SCALE_START, FALL_SCALE_END, progress);
            }
        }
        else
        {
            postImpactTimer += deltaTime;

            // Dopo un breve ritardo, pianta il seme
            if (postImpactTimer > 0.8f)
            {
                DestroyFallingSeed();
                Pianta(fallingSeed);
            }
        }
    }

    private void SpawnImpactParticles(float x, float y)
    {
        for (int i = 0; i < 25; i++)
        {
            var particle = new TutorialParticle
            {
                Position = new Vector2(x + RandomHelper.Float(-5, 5), y),
                Velocity = new Vector2(RandomHelper.Float(-100, 100), RandomHelper.Float(-150, -30)),
                Life = RandomHelper.Float(0.4f, 1.0f),
                MaxLife = 1.0f,
                Color = RandomHelper.Choose(
                    new Color(139, 90, 43, 255),
                    new Color(101, 67, 33, 255),
                    new Color(180, 140, 80, 255),
                    new Color(100, 200, 100, 255)
                ),
                Size = RandomHelper.Float(2, 5)
            };
            particles.Add(particle);
        }
    }

    private void UpdateParticles(float deltaTime)
    {
        for (int i = particles.Count - 1; i >= 0; i--)
        {
            var p = particles[i];
            p.Position += p.Velocity * deltaTime;
            p.Velocity.Y += 150f * deltaTime;
            p.Life -= deltaTime;

            if (p.Life <= 0)
                particles.RemoveAt(i);
            else
                particles[i] = p;
        }
    }

    // --- Fine animazione caduta ---

    private void Pianta(Seed seed)
    {
        Game.pianta.SetSeed(seed.type);
        Game.pianta.seedBonus = seed.stats ?? SeedDefinitions.GetTypeBonus(seed.type);
        Game.pianta.equippedItemIds = new List<string>(seed.equippedItems ?? new List<string> { null, null, null });
        Game.pianta.Reset();
        Game.pianta.SetNaturalColors(WorldManager.GetCurrentWorld());

        ItemHookCaller.CallOnStart(Game.pianta);

        Inventario.get().RemoveSeed(seed);
        Inventario.get().Save();

        Game.pianta.Stats.Salute = Game.pianta.proprieta.VitalitaMax;
        Game.pianta.Stats.Idratazione = 0.5f;
        Game.pianta.Stats.Ossigeno = 1.0f;
        Game.pianta.Stats.Metabolismo = 0.8f;
        Game.pianta.Stats.Temperatura = 20.0f;

        WaterSystem.Current = WaterSystem.Max;

        GameSave.get().data = new GameSaveData();
        GameSave.get().data.CurrentWorld = WorldManager.GetCurrentWorld();
        GameSave.get().Save();

        Game.EsciModalitaPiantaggio();
    }

    public override void Draw()
    {
        if (!active) return;

        // Overlay sfondo scuro (lascia scoperta la navbar in basso)
        int overlayH = sh - Obj_GuiBottomNavigation.BAR_HEIGHT;
        Graphics.DrawRectangle(0, 0, sw, overlayH, new Color(0, 0, 0, 200));

        // Particelle (sempre visibili)
        DrawParticles();

        if (isFalling)
        {
            DrawFalling();
            return;
        }

        // --- Header ---

        // Linea decorativa sopra il titolo
        int lineW = 80;
        int lineY = 10;
        Graphics.DrawRectangle((sw - lineW) / 2, lineY, lineW, 1, new Color(100, 220, 100, 100));

        // Titolo
        string titolo = "Scegli un Seme";
        int titoloW = titolo.Length * 9;
        Graphics.DrawText(titolo, (sw - titoloW) / 2, 15, 18, verdeChiaro);

        // Linea decorativa sotto il titolo
        Graphics.DrawRectangle((sw - lineW) / 2, 36, lineW, 1, new Color(100, 220, 100, 100));

        // Sottotitolo
        string sotto = seeds.Count > 0
            ? $"{seeds.Count} semi disponibili"
            : "Nessun seme disponibile!";
        int sottoW = sotto.Length * 5;
        Graphics.DrawText(sotto, (sw - sottoW) / 2, 42, 10, grigio);

        // Linea separatrice
        Graphics.DrawRectangle(20, 58, sw - 40, 1, new Color(80, 80, 60, 120));

        if (seeds.Count == 0)
        {
            DrawEmptyState();
            return;
        }

        // --- Sort bar ---
        int sortW = Math.Min(sw - 30, 260);
        int sortX = (sw - sortW) / 2;
        int sortY = 61;
        sorter.Draw(sortX, sortY, sortW);

        // --- Griglia semi ---
        DrawGrid();

        // --- Info seme selezionato + bottone ---
        DrawSelectedInfo();
    }

    private void DrawEmptyState()
    {
        string hint = "Apri dei pacchetti per ottenere semi";
        int hintW = hint.Length * 5;
        Graphics.DrawText(hint, (sw - hintW) / 2, sh / 2 - 10, 10, grigio);

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
    }

    private void DrawGrid()
    {
        int columns = GetColumns();
        int rows = (int)Math.Ceiling((float)seeds.Count / columns);
        int gridStartX = GetGridStartX();

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                int i = row * columns + col;
                if (i >= seeds.Count) break;

                int x = gridStartX + col * (cellSize + spacing);
                int y = startY + row * (cellSize + spacing) + scrollY;

                if (y + cellSize < startY - 10 || y > sh - 215) continue;

                Seed seed = seeds[i];
                Color rarityColor = SeedDefinitions.GetRarityColor(seed.rarity);

                Color bg = cellColor;
                Color border = borderColor;

                if (i == selectedIndex)
                {
                    bg = cellSelectedColor;
                    border = rarityColor;
                }
                else if (i == hoveredIndex)
                {
                    bg = cellHoverColor;
                    border = new Color(rarityColor.R, rarityColor.G, rarityColor.B, (byte)180);
                }

                // Ombra
                Graphics.DrawRectangleRounded(
                    new Rectangle(x + 3, y + 3, cellSize - 2, cellSize - 2),
                    0.18f, 8, innerShadow);

                // Cella
                Graphics.DrawRectangleRounded(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, bg);

                // Bordo (colorato per rarita)
                Graphics.DrawRectangleRoundedLines(
                    new Rectangle(x, y, cellSize, cellSize),
                    0.18f, 8, i == selectedIndex ? 3 : 2, border);

                // Indicatore rarita - piccola barra colorata in basso alla cella
                int barH = 3;
                int barW = cellSize - 10;
                int barX = x + 5;
                int barY = y + cellSize - barH - 3;
                byte barAlpha = (byte)(i == selectedIndex ? 255 : 150);
                Graphics.DrawRectangleRounded(
                    new Rectangle(barX, barY, barW, barH),
                    0.5f, 4,
                    new Color(rarityColor.R, rarityColor.G, rarityColor.B, barAlpha));
            }
        }

        // Disegna i semi visuali
        for (int i = 0; i < visualSeeds.Count; i++)
        {
            int col = i % columns;
            int row = i / columns;
            int y = startY + row * (cellSize + spacing) + scrollY;
            if (y + cellSize < startY - 10 || y > sh - 215) continue;

            visualSeeds[i].Draw();
        }

        // Scrollbar
        int listY = startY;
        int listH = sh - 215 - startY;
        int contentH = rows * (cellSize + spacing);
        if (contentH > listH && listH > 0)
        {
            int gridW = columns * (cellSize + spacing) - spacing;
            int sbX = gridStartX + gridW + 2;
            int sbH = Math.Max(12, listH * listH / contentH);
            float progress = (float)(-scrollY) / (contentH - listH);
            int sbY = listY + (int)(progress * (listH - sbH));
            Graphics.DrawRectangle(sbX, sbY, 2, sbH, new Color(120, 140, 100, 200));
        }
    }

    private void DrawSelectedInfo()
    {
        if (selectedIndex < 0 || selectedIndex >= seeds.Count) return;

        Seed sel = seeds[selectedIndex];
        Color rarityColor = SeedDefinitions.GetRarityColor(sel.rarity);
        string rarityName = SeedDefinitions.GetRarityName(sel.rarity);

        // Pannellino info in basso (piu alto per contenere le stats)
        int panelH = 160;
        int panelY = sh - panelH - 5 - Obj_GuiBottomNavigation.BAR_HEIGHT;
        int panelX = 10;
        int panelW = sw - 20;

        // Sfondo pannello info
        Graphics.DrawRectangleRounded(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.1f, 8, new Color(30, 35, 25, 235));
        Graphics.DrawRectangleRoundedLines(
            new Rectangle(panelX, panelY, panelW, panelH),
            0.1f, 8, 1, new Color(80, 120, 80, 150));

        // Nome del seme
        string nome = SeedDefinitions.GetSeedName(sel.type);
        int nomeW = nome.Length * 6;
        Graphics.DrawText(nome, (sw - nomeW) / 2, panelY + 5, 12, bianco);

        // Rarita
        int rarW = rarityName.Length * 4;
        Graphics.DrawText(rarityName, (sw - rarW) / 2, panelY + 19, 8, rarityColor);

        // Separatore
        Graphics.DrawRectangle(panelX + 10, panelY + 30, panelW - 20, 1, new Color(80, 100, 80, 100));

        // Statistiche compact (2 colonne)
        if (sel.stats != null)
        {
            int statsX = panelX + 12;
            int statsY = panelY + 34;
            int statsW = panelW - 24;
            SeedStatsDrawer.Draw(sel.stats, statsX, statsY, statsW, compact: true);
        }

        // Bottone "Pianta"
        int btnW = 120;
        int btnH = 22;
        int btnX = (sw - btnW) / 2;
        int btnY = panelY + panelH - btnH - 4;

        Color btnColor = buttonHovered ? buttonHoverColor : buttonColor;
        Graphics.DrawRectangleRounded(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, btnColor);
        Graphics.DrawRectangleRoundedLines(new Rectangle(btnX, btnY, btnW, btnH), 0.3f, 8, 2, verdeChiaro);

        string btnText = "Pianta!";
        int btnTextW = btnText.Length * 6;
        Graphics.DrawText(btnText, btnX + (btnW - btnTextW) / 2, btnY + 5, 12, bianco);
    }

    private void DrawFalling()
    {
        // Sfondo piu scuro durante la caduta
        Graphics.DrawRectangle(0, 0, sw, sh, new Color(0, 0, 0, 60));

        if (fallingSeedVisual != null && !fallImpacted)
        {
            // Ombra dinamica
            float progressToGround = Math.Clamp((fallSemeY + 20f) / (fallTargetY + 20f), 0f, 1f);
            float shadowScale = MathHelper.Lerp(0.3f, 1.5f, progressToGround);
            byte shadowAlpha = (byte)MathHelper.Lerp(20, 100, progressToGround);

            Graphics.DrawEllipse(
                (int)fallSemeX, (int)fallTargetY + 12,
                (int)(18 * shadowScale), (int)(6 * shadowScale),
                new Color(0, 0, 0, shadowAlpha));

            fallingSeedVisual.Draw();
        }
        else if (fallImpacted && fallingSeedVisual != null)
        {
            // Seme fermo dopo l'impatto, con leggero fade out
            byte fadeAlpha = (byte)Math.Max(0, 255 - postImpactTimer * 300);
            fallingSeedVisual.Draw();
        }
    }

    private void DrawParticles()
    {
        foreach (var p in particles)
        {
            float alpha = p.Life / p.MaxLife;
            Color col = new Color(p.Color.R, p.Color.G, p.Color.B, (byte)(p.Color.A * alpha));
            Graphics.DrawCircle((int)p.Position.X, (int)p.Position.Y, p.Size * alpha, col);
        }
    }
}
