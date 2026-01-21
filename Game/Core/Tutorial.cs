using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Rendering;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Transformations;
using Engine.Tools;

namespace Plants
{
    public enum TutorialPhase
    {
        Intro,
        SemeFluttuante,
        SemeCade,
        CrescitaIniziale,
        SpiegaIdratazione,
        SpiegaTemperatura,
        SpiegaParassiti,
        SpiegaFoglie,
        Completato,
        MostraPopupTerra,
        Fine
    }

    // Classe per salvare lo stato del tutorial
    public class TutorialSaveData
    {
        public bool Completato { get; set; } = false;
        public DateTime? DataCompletamento { get; set; } = null;
    }

    // Particella per effetti visivi
    public struct TutorialParticle
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float Life;
        public float MaxLife;
        public Color Color;
        public float Size;
    }

    public class Tutorial : GameElement
    {
        public bool isTutorialActive = false;
        private TutorialPhase currentPhase = TutorialPhase.Intro;
        
        // Animazioni
        private float animationProgress = 0f;
        private float animationSpeed = 5f;
        private float semeY = 80f;
        private float semeTargetY = 80f;
        private float semeOscillation = 0f;
        private bool semeCaduto = false;
        
        // Timing per fasi automatiche
        private float phaseTimer = 0f;
        private float currentGrowthSpeed = 0.3f; // Velocità attuale di crescita
        private float targetGrowthSpeed = 0.3f;  // Velocità target
        private float speedTransitionTime = 0f;  // Tempo per transizione velocità
        private const float SPEED_TRANSITION_DURATION = 2f; // Durata transizione

        // UI
        private bool buttonHovered = false;
        private bool skipButtonHovered = false;
        private float buttonPulse = 0f;
        private float messageAlpha = 0f;

        // Particelle per effetti
        private List<TutorialParticle> particles = new();
        private const int MAX_PARTICLES = 50;

        // Salvataggio
        private const string SAVE_FILE = "tutorial.json";
        
        // Messaggi per ogni fase
        private Dictionary<TutorialPhase, (string titolo, string[] righe)> messaggi;

        public Tutorial()
        {
            guiLayer = true;
            depth = -1000;
            
            messaggi = new Dictionary<TutorialPhase, (string, string[])>
            {
                { TutorialPhase.Intro, ("Benvenuto in Plants!", new[] {
                    "In questo gioco coltiverai piante",
                    "attraverso diversi mondi!",
                    "",
                    "La tua pianta crescera' e dovrai",
                    "proteggerla da pericoli come",
                    "disidratazione, temperature estreme",
                    "e parassiti."
                })},
                { TutorialPhase.SemeFluttuante, ("Il Seme", new[] {
                    "Ecco il tuo primo seme!",
                    "",
                    "Cliccalo per piantarlo!"
                })},
                { TutorialPhase.CrescitaIniziale, ("Crescita", new[] {
                    "Ottimo! Il seme e' stato piantato.",
                    "",
                    "Ora osserva la pianta crescere..."
                })},
                { TutorialPhase.SpiegaIdratazione, ("Idratazione", new[] {
                    "La pianta ha bisogno di ACQUA!",
                    "",
                    "Se l'idratazione scende troppo,",
                    "la pianta subira' danni.",
                    "",
                    "Usa l'annaffiatoio per innaffiare!"
                })},
                { TutorialPhase.SpiegaTemperatura, ("Temperatura", new[] {
                    "Attenzione alla TEMPERATURA!",
                    "",
                    "Troppo freddo o troppo caldo",
                    "danneggiano la pianta.",
                    "",
                    "La temperatura ideale e' 18-26 C"
                })},
                { TutorialPhase.SpiegaParassiti, ("Parassiti", new[] {
                    "I PARASSITI sono un pericolo!",
                    "",
                    "Possono infestare la pianta",
                    "e farle perdere foglie e salute.",
                    "",
                    "Tienila in salute per prevenirli!"
                })},
                { TutorialPhase.SpiegaFoglie, ("Foglie", new[] {
                    "Le FOGLIE sono vitali!",
                    "",
                    "Producono energia con la fotosintesi.",
                    "Piu' foglie = piu' energia.",
                    "",
                    "Proteggile da tempeste e parassiti!"
                })},
                { TutorialPhase.Completato, ("Tutorial Completato!", new[] {
                    "Hai imparato le basi!",
                    "",
                    "Ora sei pronto per il vero gioco.",
                    "",
                    "La prossima fermata: TERRA!"
                })}
            };
        }

        public override void Update()
        {
            if (!isTutorialActive) return;

            float deltaTime = Time.GetFrameTime();
            buttonPulse += deltaTime * 3f;
            
            // Fade in messaggi
            if (messageAlpha < 1f)
                messageAlpha += deltaTime * 2f;

            // Animazione popup
            if (currentPhase == TutorialPhase.Intro || 
                currentPhase == TutorialPhase.Completato ||
                currentPhase == TutorialPhase.MostraPopupTerra)
            {
                animationProgress = Math.Min(1f, animationProgress + deltaTime * animationSpeed);
            }

            // Aggiorna particelle
            UpdateParticles(deltaTime);

            switch (currentPhase)
            {
                case TutorialPhase.Intro:
                    CheckIntroButton();
                    CheckSkipButton();
                    break;

                case TutorialPhase.SemeFluttuante:
                    UpdateSemeFluttuante(deltaTime);
                    CheckSemeClick();
                    break;

                case TutorialPhase.SemeCade:
                    UpdateSemeCade(deltaTime);
                    break;

                case TutorialPhase.CrescitaIniziale:
                case TutorialPhase.SpiegaIdratazione:
                case TutorialPhase.SpiegaTemperatura:
                case TutorialPhase.SpiegaParassiti:
                case TutorialPhase.SpiegaFoglie:
                    if (Game.isPaused && currentPhase != TutorialPhase.CrescitaIniziale)
                    {
                        // Mostra messaggio e aspetta input utente
                        CheckMessageContinue();
                    }
                    else
                    {
                        UpdateFaseSpiegazione(deltaTime);
                    }
                    break;

                case TutorialPhase.Completato:
                    CheckCompletatoButton();
                    break;

                case TutorialPhase.MostraPopupTerra:
                    CheckTerraButton();
                    break;
            }
        }

        private void UpdateParticles(float deltaTime)
        {
            for (int i = particles.Count - 1; i >= 0; i--)
            {
                var p = particles[i];
                p.Position += p.Velocity * deltaTime;
                p.Velocity.Y += 150f * deltaTime; // Gravità
                p.Life -= deltaTime;
                
                if (p.Life <= 0)
                {
                    particles.RemoveAt(i);
                }
                else
                {
                    particles[i] = p;
                }
            }
        }

        private void SpawnSeedParticles(float x, float y)
        {
            for (int i = 0; i < 20; i++)
            {
                if (particles.Count >= MAX_PARTICLES) break;

                var particle = new TutorialParticle
                {
                    Position = new Vector2(x + RandomHelper.Float(-5, 5), y),
                    Velocity = new Vector2(RandomHelper.Float(-80, 80), RandomHelper.Float(-120, -40)),
                    Life = RandomHelper.Float(0.5f, 1.2f),
                    MaxLife = 1.2f,
                    Color = RandomHelper.Choose(
                        new Color(139, 90, 43, 255),  // Marrone seme
                        new Color(101, 67, 33, 255),  // Marrone scuro
                        new Color(180, 140, 80, 255), // Beige
                        new Color(100, 200, 100, 255) // Verde germoglio
                    ),
                    Size = RandomHelper.Float(2, 5)
                };
                particles.Add(particle);
            }
        }

        private void SpawnGrowthParticles(float x, float y)
        {
            for (int i = 0; i < 5; i++)
            {
                if (particles.Count >= MAX_PARTICLES) break;

                var particle = new TutorialParticle
                {
                    Position = new Vector2(x + RandomHelper.Float(-10, 10), y),
                    Velocity = new Vector2(RandomHelper.Float(-30, 30), RandomHelper.Float(-60, -20)),
                    Life = RandomHelper.Float(0.3f, 0.8f),
                    MaxLife = 0.8f,
                    Color = new Color(100, 255, 100, 200),
                    Size = RandomHelper.Float(1, 3)
                };
                particles.Add(particle);
            }
        }

        private void UpdateSemeFluttuante(float deltaTime)
        {
            semeOscillation += deltaTime * 2f;
            semeY = 80f + MathF.Sin(semeOscillation) * 8f;
        }

        private void CheckSemeClick()
        {
            int screenW = Rendering.camera.screenWidth;
            int semeX = screenW / 2;
            int semeSize = 24;

            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            bool hovered = mx >= semeX - semeSize && mx <= semeX + semeSize &&
                          my >= (int)semeY - semeSize && my <= (int)semeY + semeSize;

            if (hovered && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                currentPhase = TutorialPhase.SemeCade;
                semeTargetY = GameProperties.groundPosition - 20;
            }
        }

        private void UpdateSemeCade(float deltaTime)
        {
            if (!semeCaduto)
            {
                // Caduta fisica graduale
                float distanceToTarget = semeTargetY - semeY;
                float velocity = MathF.Min(distanceToTarget * 2f, 200f); // Velocità massima limitata

                semeY += velocity * deltaTime;

                if (semeY >= semeTargetY - 2f) // Tolleranza per evitare oscillazioni
                {
                    semeY = semeTargetY;
                    semeCaduto = true;
                    phaseTimer = 0f;

                    // Effetto particelle all'impatto!
                    int screenW = Rendering.camera.screenWidth;
                    SpawnSeedParticles(screenW / 2, semeTargetY);

                    // Togli la pausa - ora la pianta può crescere
                    Game.isPaused = false;

                    // Inizia crescita pianta
                    Game.pianta.Reset();
                    currentPhase = TutorialPhase.CrescitaIniziale;
                    messageAlpha = 0f;
                }
            }
        }

        private void UpdateFaseSpiegazione(float deltaTime)
        {
            // Calcola velocità di crescita attuale basata sulla percentuale
            float altezza = Game.pianta.Stats.Altezza;
            float maxAltezza = Game.pianta.Stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
            float percentuale = altezza / maxAltezza;

            // Calcola velocità target basata sulla distanza dal prossimo messaggio
            float nextThreshold = GetNextPhaseThreshold(currentPhase);
            float distanceFromThreshold = nextThreshold - percentuale;

            // Se siamo vicini a un messaggio, rallenta gradualmente
            if (distanceFromThreshold < 0.05f && nextThreshold < 1.0f)
            {
                targetGrowthSpeed = MathHelper.Lerp(0.01f, Game.growthSpeed, distanceFromThreshold / 0.05f);
            }
            else
            {
                targetGrowthSpeed = Game.growthSpeed;
            }

            // Transizione graduale della velocità
            if (MathF.Abs(currentGrowthSpeed - targetGrowthSpeed) > 0.01f)
            {
                speedTransitionTime += deltaTime;
                float t = Math.Clamp(speedTransitionTime / SPEED_TRANSITION_DURATION, 0f, 1f);
                currentGrowthSpeed = MathHelper.Lerp(currentGrowthSpeed, targetGrowthSpeed, t);
            }

            phaseTimer += deltaTime;

            // Fai crescere la pianta usando velocità corrente
            if (phaseTimer > currentGrowthSpeed)
            {
                Game.pianta.Crescita();

                // Particelle di crescita occasionali
                if (RandomHelper.Chance(20))
                {
                    int screenW = Rendering.camera.screenWidth;
                    float plantTop = GameProperties.groundPosition - Game.pianta.Stats.Altezza * 0.1f;
                    SpawnGrowthParticles(screenW / 2 + RandomHelper.Float(-20, 20), plantTop);
                }

                phaseTimer = 0f;
            }

            // Verifica se dobbiamo passare alla prossima fase
            TutorialPhase newPhase = currentPhase;

            if (percentuale < 0.2f)
                newPhase = TutorialPhase.CrescitaIniziale;
            else if (percentuale < 0.4f)
                newPhase = TutorialPhase.SpiegaIdratazione;
            else if (percentuale < 0.6f)
                newPhase = TutorialPhase.SpiegaTemperatura;
            else if (percentuale < 0.8f)
                newPhase = TutorialPhase.SpiegaParassiti;
            else if (percentuale < 0.95f)
                newPhase = TutorialPhase.SpiegaFoglie;
            else
            {
                newPhase = TutorialPhase.Completato;
                animationProgress = 0f;
                Game.isPaused = true; // Ferma completamente quando completato
            }

            if (newPhase != currentPhase)
            {
                // Quando cambiamo fase messaggio, metti in pausa per mostrare il messaggio
                Game.isPaused = true;
                speedTransitionTime = 0f;
                currentPhase = newPhase;
                messageAlpha = 0f;

                // Se è una fase messaggio, assicurati che sia in pausa
                if (newPhase != TutorialPhase.CrescitaIniziale)
                {
                    Game.isPaused = true;
                }
            }
        }

        private float GetNextPhaseThreshold(TutorialPhase current)
        {
            return current switch
            {
                TutorialPhase.CrescitaIniziale => 0.2f,
                TutorialPhase.SpiegaIdratazione => 0.4f,
                TutorialPhase.SpiegaTemperatura => 0.6f,
                TutorialPhase.SpiegaParassiti => 0.8f,
                TutorialPhase.SpiegaFoglie => 0.95f,
                _ => 1.0f
            };
        }

        private void CheckIntroButton()
        {
            if (animationProgress < 0.9f) return;

            var (bx, by, bw, bh) = GetButtonRect();
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            buttonHovered = mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;

            if (buttonHovered && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                currentPhase = TutorialPhase.SemeFluttuante;
                messageAlpha = 0f;
                animationProgress = 0f;
                //ResumeGrowth(); // Riprendi la crescita (anche se non c'è pianta ancora)
            }
        }

        private void CheckSkipButton()
        {
            if (animationProgress < 0.9f) return;

            var (bx, by, bw, bh) = GetSkipButtonRect();
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            skipButtonHovered = mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;

            if (skipButtonHovered && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                SkipTutorial();
            }
        }

        private void SkipTutorial()
        {
            // Salva che il tutorial è stato completato (anche se saltato)
            SaveTutorialCompleted();
            
            // Togli la pausa
            Game.isPaused = false;
            
            // Vai direttamente su Terra
            WorldManager.SetCurrentWorld(WorldType.Terra);
            Game.pianta.Reset();
            Game.pianta.SetNaturalColors(WorldType.Terra);
            
            // Fai crescere un po' la pianta
            for (int i = 0; i < 400; i++)
            {
                Game.pianta.Crescita();
            }
            
            isTutorialActive = false;
            currentPhase = TutorialPhase.Fine;
        }

        private void CheckCompletatoButton()
        {
            if (animationProgress < 0.9f) return;

            var (bx, by, bw, bh) = GetButtonRect();
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            buttonHovered = mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;

            if (buttonHovered && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                currentPhase = TutorialPhase.MostraPopupTerra;
                animationProgress = 0f;
                messageAlpha = 0f;
            }
        }

        private void CheckMessageContinue()
        {
            // Mostra testo "Clicca per continuare" e aspetta input
            if (Input.IsMouseButtonPressed(MouseButton.Left))
            {
                ResumeGrowth();
            }
        }

        private void CheckTerraButton()
        {
            if (animationProgress < 0.9f) return;

            var (bx, by, bw, bh) = GetButtonRect();
            int mx = Input.GetMouseX();
            int my = Input.GetMouseY();

            buttonHovered = mx >= bx && mx <= bx + bw && my >= by && my <= by + bh;

            if (buttonHovered && Input.IsMouseButtonPressed(MouseButton.Left))
            {
                // Salva completamento
                SaveTutorialCompleted();

                // Togli la pausa
                Game.isPaused = false;

                // Vai su Terra
                WorldManager.SetCurrentWorld(WorldType.Terra);
                Game.pianta.Reset();
                Game.pianta.SetNaturalColors(WorldType.Terra);

                isTutorialActive = false;
                currentPhase = TutorialPhase.Fine;
            }
        }

        private (int x, int y, int w, int h) GetButtonRect()
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;
            int panelW = 260;
            int panelH = 280;
            int panelX = (screenW - panelW) / 2;
            int panelY = (screenH - panelH) / 2;
            
            int buttonW = 140;
            int buttonH = 32;
            int buttonX = panelX + (panelW - buttonW) / 2;
            int buttonY = panelY + panelH - 55;
            
            return (buttonX, buttonY, buttonW, buttonH);
        }

        private (int x, int y, int w, int h) GetSkipButtonRect()
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;
            int panelW = 260;
            int panelH = 280;
            int panelX = (screenW - panelW) / 2;
            int panelY = (screenH - panelH) / 2;
            
            int buttonW = 90;
            int buttonH = 22;
            int buttonX = panelX + (panelW - buttonW) / 2;
            int buttonY = panelY + panelH - 22;
            
            return (buttonX, buttonY, buttonW, buttonH);
        }

        public override void Draw()
        {
            if (!isTutorialActive) return;

            // Disegna sempre le particelle
            DrawParticles();

            switch (currentPhase)
            {
                case TutorialPhase.Intro:
                    DrawPopup("Benvenuto in Plants!", GetIntroText(), "INIZIA", true, true);
                    break;

                case TutorialPhase.SemeFluttuante:
                    DrawSeme();
                    DrawMessageBox("Il Seme", new[] { "Clicca il seme per piantarlo!" });
                    break;

                case TutorialPhase.SemeCade:
                    DrawSeme();
                    break;

                case TutorialPhase.CrescitaIniziale:
                case TutorialPhase.SpiegaIdratazione:
                case TutorialPhase.SpiegaTemperatura:
                case TutorialPhase.SpiegaParassiti:
                case TutorialPhase.SpiegaFoglie:
                    if (Game.isPaused && currentPhase != TutorialPhase.CrescitaIniziale)
                    {
                        // Mostra messaggio in pausa con "Clicca per continuare"
                        if (messaggi.TryGetValue(currentPhase, out var msg))
                        {
                            DrawMessageBox(msg.titolo, msg.righe);
                            DrawStatHighlight();
                            DrawContinuePrompt();
                        }
                    }
                    else
                    {
                        // Durante la crescita, mostra solo l'highlight della statistica
                        DrawStatHighlight();
                    }
                    break;

                case TutorialPhase.Completato:
                    DrawPopup("Tutorial Completato!", GetCompletatoText(), "CONTINUA", true, false);
                    break;

                case TutorialPhase.MostraPopupTerra:
                    DrawPopupTerra();
                    break;
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

        private string[] GetIntroText()
        {
            return new[] {
                "",
                "In questo gioco coltiverai",
                "piante attraverso diversi mondi!",
                "",
                "Proteggi la tua pianta da:",
                "- Disidratazione",
                "- Temperature estreme", 
                "- Parassiti",
                ""
            };
        }

        private string[] GetCompletatoText()
        {
            return new[] {
                "",
                "Hai imparato le basi!",
                "",
                "Sei pronto per iniziare",
                "la tua avventura sulla Terra.",
                ""
            };
        }

        private void DrawPopup(string title, string[] lines, string buttonText, bool showOverlay, bool showSkip)
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;

            if (showOverlay)
            {
                byte overlayAlpha = (byte)(150 * animationProgress);
                Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, overlayAlpha));
            }

            float eased = EaseOutBack(animationProgress);
            int panelW = (int)(260 * eased);
            int panelH = (int)(280 * eased);
            int panelX = (screenW - panelW) / 2;
            int panelY = (screenH - panelH) / 2;

            if (panelW < 50) return;

            // Sfondo pannello
            Color panelBg = new Color(35, 45, 35, 240);
            Color panelBorder = new Color(100, 180, 100, 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(panelX, panelY, panelW, panelH),
                0.1f, 8, panelBg
            );
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(panelX, panelY, panelW, panelH),
                0.1f, 8, 2, panelBorder
            );

            if (animationProgress < 0.5f) return;

            // Titolo
            int titleWidth = title.Length * 8;
            Graphics.DrawText(title, panelX + (panelW - titleWidth) / 2, panelY + 15, 16, new Color(150, 255, 150, 255));

            // Linee di testo
            int startY = panelY + 45;
            for (int i = 0; i < lines.Length; i++)
            {
                int lineWidth = lines[i].Length * 6;
                Graphics.DrawText(lines[i], panelX + (panelW - lineWidth) / 2, startY + i * 16, 12, new Color(220, 220, 220, 255));
            }

            // Bottone principale
            var (bx, by, bw, bh) = GetButtonRect();
            
            float pulse = (MathF.Sin(buttonPulse) + 1f) * 0.5f;
            Color buttonBg = buttonHovered
                ? new Color(80, 180, 80, 255)
                : new Color((byte)(50 + pulse * 20), (byte)(140 + pulse * 30), (byte)(50 + pulse * 20), 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(bx, by, bw, bh),
                0.3f, 8, buttonBg
            );
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(bx, by, bw, bh),
                0.3f, 8, 2, buttonHovered ? Color.White : new Color(120, 220, 120, 255)
            );

            int textWidth = buttonText.Length * 8;
            Graphics.DrawText(buttonText, bx + (bw - textWidth) / 2, by + 8, 14, Color.White);

            // Bottone Salta (solo se richiesto)
            if (showSkip)
            {
                var (sx, sy, sw, sh) = GetSkipButtonRect();
                
                Color skipBg = skipButtonHovered
                    ? new Color(80, 80, 90, 200)
                    : new Color(50, 50, 60, 150);

                Graphics.DrawRectangleRounded(
                    new Rectangle(sx, sy, sw, sh),
                    0.3f, 6, skipBg
                );

                Color skipTextCol = skipButtonHovered
                    ? new Color(255, 255, 255, 255)
                    : new Color(150, 150, 160, 200);

                Graphics.DrawText("Salta", sx + (sw - 25) / 2, sy + 4, 10, skipTextCol);
            }
        }

        private void DrawPopupTerra()
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;

            byte overlayAlpha = (byte)(150 * animationProgress);
            Graphics.DrawRectangle(0, 0, screenW, screenH, new Color(0, 0, 0, overlayAlpha));

            float eased = EaseOutBack(animationProgress);
            int panelW = (int)(180 * eased);
            int panelH = (int)(200 * eased);
            int panelX = (screenW - panelW) / 2;
            int panelY = (screenH - panelH) / 2;

            if (panelW < 50) return;

            Color panelBg = new Color(30, 40, 60, 240);
            Color panelBorder = new Color(100, 150, 255, 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(panelX, panelY, panelW, panelH),
                0.1f, 8, panelBg
            );
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(panelX, panelY, panelW, panelH),
                0.1f, 8, 2, panelBorder
            );

            if (animationProgress < 0.5f) return;

            // Icona Terra (cerchio blu/verde)
            int iconX = panelX + panelW / 2;
            int iconY = panelY + 45;
            Graphics.DrawCircle(iconX, iconY, 20, new Color(80, 150, 255, 255));
            Graphics.DrawCircle(iconX - 5, iconY - 3, 8, new Color(100, 200, 100, 255));
            Graphics.DrawCircle(iconX + 8, iconY + 5, 5, new Color(100, 200, 100, 255));

            // Titolo
            Graphics.DrawText("TERRA", panelX + (panelW - 36) / 2, panelY + 75, 14, new Color(100, 200, 100, 255));
            Graphics.DrawText("Primo Mondo", panelX + (panelW - 66) / 2, panelY + 95, 10, new Color(180, 180, 200, 255));

            // Descrizione
            string[] desc = new[] {
                "Il nostro pianeta.",
                "Condizioni ideali",
                "per iniziare!",
                "",
                "Difficolta: Facile"
            };

            int startY = panelY + 115;
            for (int i = 0; i < desc.Length; i++)
            {
                int lineW = desc[i].Length * 5;
                Color col = i == 4 ? new Color(100, 200, 100, 255) : new Color(200, 200, 200, 255);
                Graphics.DrawText(desc[i], panelX + (panelW - lineW) / 2, startY + i * 12, 9, col);
            }

            // Bottone
            var (bx, by, bw, bh) = GetButtonRect();
            // Riposiziona per questo popup più piccolo
            by = panelY + panelH - 35;
            
            float pulse = (MathF.Sin(buttonPulse) + 1f) * 0.5f;
            Color buttonBg = buttonHovered
                ? new Color(80, 150, 80, 255)
                : new Color((byte)(50 + pulse * 15), (byte)(120 + pulse * 25), (byte)(50 + pulse * 15), 255);

            Graphics.DrawRectangleRounded(
                new Rectangle(bx, by, bw, bh),
                0.3f, 8, buttonBg
            );
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(bx, by, bw, bh),
                0.3f, 8, 2, buttonHovered ? Color.White : new Color(100, 200, 100, 255)
            );

            Graphics.DrawText("VIAGGIA", bx + (bw - 42) / 2, by + 8, 11, Color.White);
        }

        private void DrawSeme()
        {
            int screenW = Rendering.camera.screenWidth;
            int semeX = screenW / 2;

            // Ombra dinamica - solo se il seme è abbastanza alto
            if (semeY > 100)
            {
                float progressToGround = Math.Clamp((semeY - 80f) / (semeTargetY - 80f), 0f, 1f);
                float shadowScale = MathHelper.Lerp(0.3f, 1.2f, progressToGround);
                byte shadowAlpha = (byte)MathHelper.Lerp(30, 120, progressToGround);

                Graphics.DrawEllipse(semeX, (int)semeTargetY + 8, (int)(15 * shadowScale), (int)(5 * shadowScale),
                    new Color(0, 0, 0, shadowAlpha));
            }

            // Seme (forma ovale marrone)
            Color semeColor1 = new Color(139, 90, 43, 255);
            Color semeColor2 = new Color(101, 67, 33, 255);

            Graphics.DrawEllipse(semeX, (int)semeY, 10, 14, semeColor1);
            Graphics.DrawEllipse(semeX - 2, (int)semeY - 2, 6, 8, semeColor2);

            // Brillantino
            Graphics.DrawCircle(semeX - 3, (int)semeY - 5, 2, new Color(255, 255, 255, 150));

            // Testo "Cliccami!" con animazione
            if (currentPhase == TutorialPhase.SemeFluttuante)
            {
                float alpha = (MathF.Sin(buttonPulse * 2) + 1f) * 0.5f;
                Color textCol = new Color(255, 255, 100, (byte)(180 + alpha * 75));
                Graphics.DrawText("Cliccami!", semeX - 28, (int)semeY - 35, 12, textCol);

                // Freccia che punta al seme
                float arrowY = semeY - 45 + MathF.Sin(buttonPulse * 3) * 3;
                Graphics.DrawTriangle(
                    new Vector2(semeX, arrowY + 8),
                    new Vector2(semeX - 6, arrowY),
                    new Vector2(semeX + 6, arrowY),
                    textCol
                );
            }
        }

        private void DrawMessageBox(string title, string[] lines)
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;

            int boxWidth = 180;
            int boxHeight = 20 + lines.Length * 13;
            int boxX = (screenW - boxWidth) / 2;
            int boxY = screenH - boxHeight - 20;

            byte alpha = (byte)(220 * messageAlpha);
            Color boxBg = new Color(30, 35, 45, alpha);
            Color boxBorder = new Color(100, 180, 255, (byte)(255 * messageAlpha));

            Graphics.DrawRectangleRounded(
                new Rectangle(boxX, boxY, boxWidth, boxHeight),
                0.15f, 8, boxBg
            );
            Graphics.DrawRectangleRoundedLines(
                new Rectangle(boxX, boxY, boxWidth, boxHeight),
                0.15f, 8, 2, boxBorder
            );

            // Titolo
            Color titleColor = new Color(255, 220, 100, (byte)(255 * messageAlpha));
            int titleW = title.Length * 6;
            Graphics.DrawText(title, boxX + (boxWidth - titleW) / 2, boxY + 8, 11, titleColor);

            // Linee
            Color textColor = new Color(220, 220, 220, (byte)(255 * messageAlpha));
            for (int i = 0; i < lines.Length; i++)
            {
                int lineW = lines[i].Length * 5;
                Graphics.DrawText(lines[i], boxX + (boxWidth - lineW) / 2, boxY + 24 + i * 13, 9, textColor);
            }
        }

        private void DrawStatHighlight()
        {
            // Evidenzia la statistica rilevante durante la spiegazione
            int screenW = Rendering.camera.screenWidth;
            
            Color highlight = new Color(255, 255, 100, (byte)(60 + MathF.Sin(buttonPulse * 2) * 30));
            
            switch (currentPhase)
            {
                case TutorialPhase.SpiegaIdratazione:
                    DrawWaterDropIcon(screenW - 40, 60, highlight);
                    break;
                case TutorialPhase.SpiegaTemperatura:
                    DrawThermometerIcon(screenW - 40, 60, highlight);
                    break;
                case TutorialPhase.SpiegaParassiti:
                    DrawBugIcon(screenW - 40, 60, highlight);
                    break;
                case TutorialPhase.SpiegaFoglie:
                    DrawLeafIcon(screenW - 40, 60, highlight);
                    break;
            }
        }

        private void DrawWaterDropIcon(int x, int y, Color color)
        {
            Graphics.DrawCircle(x, y + 8, 12, color);
            Graphics.DrawTriangle(
                new Vector2(x, y - 10),
                new Vector2(x - 10, y + 5),
                new Vector2(x + 10, y + 5),
                color
            );
        }

        private void DrawThermometerIcon(int x, int y, Color color)
        {
            Graphics.DrawRectangle(x - 4, y - 15, 8, 25, color);
            Graphics.DrawCircle(x, y + 15, 8, color);
        }

        private void DrawBugIcon(int x, int y, Color color)
        {
            Graphics.DrawEllipse(x, y, 10, 6, color);
            Graphics.DrawCircle(x - 8, y, 4, color);
        }

        private void DrawLeafIcon(int x, int y, Color color)
        {
            Graphics.DrawEllipse(x, y, 8, 14, color);
            Graphics.DrawLineEx(new Vector2(x, y + 14), new Vector2(x, y - 5), 2, color);
        }

        private void DrawContinuePrompt()
        {
            int screenW = Rendering.camera.screenWidth;
            int screenH = Rendering.camera.screenHeight;

            string continueText = "Clicca per continuare";
            int textWidth = continueText.Length * 7;
            int textX = (screenW - textWidth) / 2;
            int textY = screenH - 50;

            // Sfondo semi-trasparente
            Graphics.DrawRectangle(textX - 10, textY - 5, textWidth + 20, 20, new Color(0, 0, 0, 120));

            // Testo lampeggiante
            float alpha = (MathF.Sin(buttonPulse * 2) + 1f) * 0.5f;
            Color textColor = new Color(255, 255, 255, (byte)(200 + alpha * 55));
            Graphics.DrawText(continueText, textX, textY, 12, textColor);
        }

        private float EaseOutBack(float x)
        {
            const float c1 = 1.70158f;
            const float c3 = c1 + 1;
            return 1 + c3 * MathF.Pow(x - 1, 3) + c1 * MathF.Pow(x - 1, 2);
        }

        // === SALVATAGGIO ===

        public static bool IsTutorialCompleted()
        {
            var data = SaveHelper.Load<TutorialSaveData>(SAVE_FILE);
            return data?.Completato ?? false;
        }

        private void SaveTutorialCompleted()
        {
            var data = new TutorialSaveData
            {
                Completato = true,
                DataCompletamento = DateTime.Now
            };
            SaveHelper.Save(SAVE_FILE, data);
        }

        public static void ResetTutorialProgress()
        {
            SaveHelper.Delete(SAVE_FILE);
        }

        // === AVVIO ===

        private void ResumeGrowth()
        {
            Game.isPaused = false;
            currentGrowthSpeed = Game.growthSpeed;
            targetGrowthSpeed = Game.growthSpeed;
            speedTransitionTime = 0f;
        }

        public void StartTutorial()
        {
            Console.WriteLine($"[TUTORIAL] StartTutorial() chiamato");
            // Controlla se il tutorial è già stato completato
            if (IsTutorialCompleted())
            {
                Console.WriteLine($"[TUTORIAL] Tutorial completato rilevato - Impostazione mondo a Terra");
                // Salta direttamente a Terra
                WorldManager.SetCurrentWorld(WorldType.Terra);
                Game.pianta.SetNaturalColors(WorldType.Terra);

                // Forza il salvataggio del mondo Terra eliminando prima il vecchio file
                Console.WriteLine($"[TUTORIAL] Eliminazione file salvataggio esistente...");
                GameSaveManager.DeleteSaveFile();

                // Salva il gioco con il mondo corretto dopo il completamento del tutorial
                Console.WriteLine($"[TUTORIAL] Salvataggio completamento tutorial - Mondo: Terra");
                GameSaveManager.SaveGame(
                    WorldType.Terra,
                    WorldManager.GetCurrentWorldDifficulty(),
                    WeatherManager.GetCurrentWeather(),
                    FaseGiorno.GetCurrentPhase()
                );
                Console.WriteLine($"[TUTORIAL] Salvataggio completato");

                isTutorialActive = false;
                currentPhase = TutorialPhase.Fine;
                return;
            }

            isTutorialActive = true;
            currentPhase = TutorialPhase.Intro;
            animationProgress = 0f;
            messageAlpha = 0f;
            semeY = 80f;
            semeCaduto = false;
            phaseTimer = 0f;
            particles.Clear();
            currentGrowthSpeed = Game.growthSpeed;
            targetGrowthSpeed = Game.growthSpeed;
            speedTransitionTime = 0f;

            // Metti in pausa il gioco durante il tutorial iniziale
            Game.isPaused = true;
        }
    }
}
