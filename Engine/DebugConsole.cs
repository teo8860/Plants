using Raylib_CSharp;
using Raylib_CSharp.Colors;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public static class DebugConsole
{
    private static bool isOpen = false;
    private static string inputText = "";
    private static List<string> outputLines = new();
    private static int maxOutputLines = 12;

    private static float cursorBlinkTimer = 0f;
    private static bool cursorVisible = true;

    // Command history
    private static List<string> history = new();
    private static int historyIndex = -1;
    private static string savedInput = "";

    // Autocomplete ghost text
    private static string currentSuggestion = "";

    // Selection picker
    private static bool pickerActive = false;
    private static List<string> pickerOptions = new();
    private static int pickerIndex = 0;
    private static string pickerPrefix = "";

    // All commands for ghost-text autocomplete
    private static readonly List<string> allCommands = new()
    {
        "reset all",
        "reset inventory",
        "add seed",
        "add leaf",
        "add essence",
        "weather set",
        "stage set",
        "world set",
        "plant grow",
        "tick set",
        "tick reset",
        "kill",
        "godmode",
        "minigame start",
        "minigame end",
        "help"
    };

    // Commands that open a picker when entered without arguments
    private static readonly Dictionary<string, Func<string[]>> pickerCommands = new()
    {
        { "reset", () => new[] { "all", "inventory" } },
        { "add", () => new[] { "seed", "leaf", "essence" } },
        { "tick", () => new[] { "set", "reset" } },
        { "weather set", () => Enum.GetNames(typeof(Weather)) },
        { "world set", () => Enum.GetNames(typeof(WorldType)) },
        { "stage set", () => Enumerable.Range(1, 20).Select(i => i.ToString()).ToArray() },
        { "plant grow", () => new[] { "1", "5", "10", "25", "50", "100" } },
        { "tick set", () => new[] { "100", "250", "500", "1000", "2000", "5000" } },
        { "add seed", () => new[] { "1", "5", "10", "25", "50", "100" } },
        { "add leaf", () => new[] { "1", "5", "10", "25", "50", "100" } },
        { "add essence", () => new[] { "10", "50", "100", "500", "1000" } },
        { "minigame", () => new[] { "start", "end" } },
        { "minigame start", () => Enum.GetNames(typeof(TipoMinigioco)) },
    };

    // Enum values for ghost-text autocomplete
    private static readonly Dictionary<string, string[]> enumValues = new()
    {
        { "weather set", Enum.GetNames(typeof(Weather)) },
        { "world set", Enum.GetNames(typeof(WorldType)) },
    };

    // Tick system
    private static double originalTickInterval = 1000;

    // God mode
    public static bool GodMode = false;

    public static bool IsOpen => isOpen;

    public static void Update()
    {
        // Toggle with Tab
        if (Input.IsKeyPressed(KeyboardKey.Tab))
        {
            if (pickerActive)
            {
                ClosePicker();
            }
            else
            {
                isOpen = !isOpen;
                if (isOpen)
                {
                    inputText = "";
                    currentSuggestion = "";
                    historyIndex = -1;
                }
            }
            return;
        }

        if (!isOpen) return;

        // Blink cursor
        cursorBlinkTimer += Time.GetFrameTime();
        if (cursorBlinkTimer >= 0.5f)
        {
            cursorVisible = !cursorVisible;
            cursorBlinkTimer = 0f;
        }

        // --- Picker mode ---
        if (pickerActive)
        {
            UpdatePicker();
            return;
        }

        // --- Normal input mode ---

        // Up/Down for command history
        if (Input.IsKeyPressed(KeyboardKey.Up))
        {
            if (history.Count > 0)
            {
                if (historyIndex == -1)
                {
                    savedInput = inputText;
                    historyIndex = history.Count - 1;
                }
                else if (historyIndex > 0)
                {
                    historyIndex--;
                }
                inputText = history[historyIndex];
                UpdateSuggestion();
            }
            return;
        }

        if (Input.IsKeyPressed(KeyboardKey.Down))
        {
            if (historyIndex >= 0)
            {
                historyIndex++;
                if (historyIndex >= history.Count)
                {
                    historyIndex = -1;
                    inputText = savedInput;
                }
                else
                {
                    inputText = history[historyIndex];
                }
                UpdateSuggestion();
            }
            return;
        }

        // Right arrow to accept ghost suggestion
        if (Input.IsKeyPressed(KeyboardKey.Right) && currentSuggestion.Length > 0)
        {
            inputText = currentSuggestion;
            currentSuggestion = "";
        }

        // Handle text input
        int charPressed = Input.GetCharPressed();
        while (charPressed > 0)
        {
            if (charPressed >= 32 && charPressed <= 126)
            {
                inputText += (char)charPressed;
                historyIndex = -1;
                UpdateSuggestion();
            }
            charPressed = Input.GetCharPressed();
        }

        // Backspace
        if (Input.IsKeyPressed(KeyboardKey.Backspace) || Input.IsKeyPressedRepeat(KeyboardKey.Backspace))
        {
            if (inputText.Length > 0)
            {
                inputText = inputText[..^1];
                historyIndex = -1;
                UpdateSuggestion();
            }
        }

        // Enter to execute (or open picker if args missing)
        if (Input.IsKeyPressed(KeyboardKey.Enter))
        {
            string trimmed = inputText.Trim();
            if (trimmed.Length > 0)
            {
                if (TryOpenPicker(trimmed))
                    return;

                AddToHistory(trimmed);
                ExecuteCommand(trimmed);
                inputText = "";
                currentSuggestion = "";
                historyIndex = -1;
            }
        }

        // Escape to close
        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            isOpen = false;
        }
    }

    private static void UpdatePicker()
    {
        if (Input.IsKeyPressed(KeyboardKey.Up) || Input.IsKeyPressedRepeat(KeyboardKey.Up))
        {
            pickerIndex--;
            if (pickerIndex < 0) pickerIndex = pickerOptions.Count - 1;
        }

        if (Input.IsKeyPressed(KeyboardKey.Down) || Input.IsKeyPressedRepeat(KeyboardKey.Down))
        {
            pickerIndex++;
            if (pickerIndex >= pickerOptions.Count) pickerIndex = 0;
        }

        if (Input.IsKeyPressed(KeyboardKey.Enter))
        {
            string selected = pickerOptions[pickerIndex];
            string fullCommand = pickerPrefix.Length > 0 ? pickerPrefix + " " + selected : selected;
            ClosePicker();

            // If the selected option itself has sub-options, open another picker
            if (TryOpenPicker(fullCommand))
            {
                inputText = fullCommand;
                return;
            }

            AddToHistory(fullCommand);
            ExecuteCommand(fullCommand);
            inputText = "";
            currentSuggestion = "";
            historyIndex = -1;
        }

        if (Input.IsKeyPressed(KeyboardKey.Escape))
        {
            ClosePicker();
        }
    }

    private static bool TryOpenPicker(string trimmedInput)
    {
        string lower = trimmedInput.ToLower();

        // Check from longest prefix to shortest to match most specific first
        var sorted = pickerCommands.Keys.OrderByDescending(k => k.Length);
        foreach (var key in sorted)
        {
            if (lower == key)
            {
                // Exact match with no extra args → open picker
                OpenPicker(trimmedInput, pickerCommands[key]());
                return true;
            }
        }
        return false;
    }

    private static void OpenPicker(string prefix, string[] options)
    {
        pickerActive = true;
        pickerOptions = new List<string>(options);
        pickerIndex = 0;
        pickerPrefix = prefix;
        inputText = prefix;
    }

    private static void ClosePicker()
    {
        pickerActive = false;
        pickerOptions.Clear();
        pickerIndex = 0;
        pickerPrefix = "";
    }

    private static void AddToHistory(string command)
    {
        if (history.Count == 0 || history[^1] != command)
            history.Add(command);
        if (history.Count > 50)
            history.RemoveAt(0);
    }

    public static void Draw()
    {
        if (!isOpen) return;

        int sw = Rendering.camera.screenWidth;
        int sh = Rendering.camera.screenHeight;

        int panelX = 10;
        int panelY = 10;
        int panelW = sw - 20;
        int panelH = sh - 20;

        // Dark transparent background
        Graphics.DrawRectangle(panelX, panelY, panelW, panelH, new Color(10, 10, 15, 210));
        Graphics.DrawRectangleLines(panelX, panelY, panelW, panelH, new Color(80, 200, 120, 180));

        // Title
        Graphics.DrawText("DEBUG CONSOLE", panelX + 8, panelY + 6, 12, new Color(80, 200, 120, 255));
        string hint = pickerActive ? "[UP/DOWN] select  [ENTER] confirm  [ESC] cancel" : "[TAB] close  [RIGHT] autocomplete  [UP/DOWN] history";
        Graphics.DrawText(hint, panelX + 130, panelY + 8, 8, new Color(120, 120, 130, 200));

        // Output lines
        int lineY = panelY + 26;
        int lineHeight = 13;
        int startLine = Math.Max(0, outputLines.Count - maxOutputLines);
        for (int i = startLine; i < outputLines.Count; i++)
        {
            string line = outputLines[i];
            Color lineColor = new Color(200, 200, 210, 220);

            if (line.StartsWith("[OK]"))
                lineColor = new Color(80, 220, 100, 255);
            else if (line.StartsWith("[ERR]"))
                lineColor = new Color(255, 80, 80, 255);
            else if (line.StartsWith(">"))
                lineColor = new Color(150, 150, 160, 180);

            Graphics.DrawText(line, panelX + 10, lineY, 10, lineColor);
            lineY += lineHeight;
        }

        // Input line
        int inputY = panelY + panelH - 24;
        Graphics.DrawRectangle(panelX + 2, inputY - 2, panelW - 4, 20, new Color(20, 20, 25, 240));
        Graphics.DrawText("> " + inputText, panelX + 8, inputY + 2, 10, new Color(220, 220, 230, 255));

        // Cursor (hide during picker)
        if (cursorVisible && !pickerActive)
        {
            int cursorX = panelX + 8 + ("> " + inputText).Length * 6;
            Graphics.DrawText("_", cursorX, inputY + 2, 10, new Color(80, 200, 120, 255));
        }

        // Ghost text suggestion (hide during picker)
        if (!pickerActive && currentSuggestion.Length > inputText.Length && inputText.Length > 0)
        {
            string ghostPart = currentSuggestion[inputText.Length..];
            int ghostX = panelX + 8 + ("> " + inputText).Length * 6;
            Graphics.DrawText(ghostPart, ghostX, inputY + 2, 10, new Color(80, 200, 120, 90));
        }

        // Picker box (drawn above input line)
        if (pickerActive && pickerOptions.Count > 0)
        {
            DrawPicker(panelX, inputY, panelW);
        }
    }

    private static void DrawPicker(int panelX, int inputY, int panelW)
    {
        int itemH = 16;
        int maxVisible = Math.Min(pickerOptions.Count, 14);
        int pickerH = maxVisible * itemH + 8;
        int pickerW = panelW - 20;
        int pickerX = panelX + 10;
        int pickerY = inputY - pickerH - 6;

        // Scroll window: keep selected item visible
        int scrollOffset = 0;
        if (pickerOptions.Count > maxVisible)
        {
            scrollOffset = pickerIndex - maxVisible / 2;
            scrollOffset = Math.Clamp(scrollOffset, 0, pickerOptions.Count - maxVisible);
        }

        // Background
        Graphics.DrawRectangle(pickerX, pickerY, pickerW, pickerH, new Color(18, 18, 24, 240));
        Graphics.DrawRectangleLines(pickerX, pickerY, pickerW, pickerH, new Color(80, 200, 120, 140));

        // Items
        for (int i = 0; i < maxVisible; i++)
        {
            int idx = scrollOffset + i;
            if (idx >= pickerOptions.Count) break;

            int itemY = pickerY + 4 + i * itemH;
            bool selected = idx == pickerIndex;

            if (selected)
            {
                // Highlight bar
                Graphics.DrawRectangle(pickerX + 2, itemY, pickerW - 4, itemH, new Color(80, 200, 120, 50));

                // Left accent bar
                Graphics.DrawRectangle(pickerX + 2, itemY, 3, itemH, new Color(80, 200, 120, 220));

                // Arrow indicator
                Graphics.DrawText(">", pickerX + 8, itemY + 2, 10, new Color(80, 200, 120, 255));
            }

            Color textCol = selected
                ? new Color(255, 255, 255, 255)
                : new Color(170, 170, 180, 200);

            Graphics.DrawText(pickerOptions[idx], pickerX + 22, itemY + 2, 10, textCol);
        }

        // Scroll indicators
        if (scrollOffset > 0)
        {
            Graphics.DrawText("...", pickerX + pickerW - 24, pickerY + 2, 8, new Color(120, 120, 130, 160));
        }
        if (scrollOffset + maxVisible < pickerOptions.Count)
        {
            Graphics.DrawText("...", pickerX + pickerW - 24, pickerY + pickerH - 12, 8, new Color(120, 120, 130, 160));
        }
    }

    private static void UpdateSuggestion()
    {
        currentSuggestion = "";
        if (inputText.Length == 0) return;

        string lower = inputText.ToLower();

        // First try matching commands
        foreach (var cmd in allCommands)
        {
            if (cmd.StartsWith(lower) && cmd != lower)
            {
                currentSuggestion = cmd;
                return;
            }
        }

        // Try matching enum values for commands that take them
        foreach (var kv in enumValues)
        {
            if (lower.StartsWith(kv.Key + " "))
            {
                string partial = inputText[(kv.Key.Length + 1)..];
                if (partial.Length == 0) continue;
                foreach (var val in kv.Value)
                {
                    if (val.StartsWith(partial, StringComparison.OrdinalIgnoreCase) && !val.Equals(partial, StringComparison.OrdinalIgnoreCase))
                    {
                        currentSuggestion = kv.Key + " " + val;
                        return;
                    }
                }
            }
        }
    }

    private static void ExecuteCommand(string rawInput)
    {
        outputLines.Add("> " + rawInput);
        string input = rawInput.ToLower().Trim();

        try
        {
            if (input == "help")
            {
                PrintHelp();
            }
            else if (input == "reset all")
            {
                CmdResetAll();
            }
            else if (input == "reset inventory")
            {
                CmdResetInventory();
            }
            else if (input.StartsWith("add seed"))
            {
                int count = ParseIntArg(input, "add seed", 1);
                CmdAddSeed(count);
            }
            else if (input.StartsWith("add leaf"))
            {
                int count = ParseIntArg(input, "add leaf", 1);
                CmdAddLeaf(count);
            }
            else if (input.StartsWith("add essence"))
            {
                int count = ParseIntArg(input, "add essence", 1);
                CmdAddEssence(count);
            }
            else if (input.StartsWith("weather set"))
            {
                string arg = ParseStringArg(input, "weather set");
                CmdWeatherSet(arg);
            }
            else if (input.StartsWith("stage set"))
            {
                int val = ParseIntArg(input, "stage set", -1);
                CmdStageSet(val);
            }
            else if (input.StartsWith("world set"))
            {
                string arg = ParseStringArg(input, "world set");
                CmdWorldSet(arg);
            }
            else if (input.StartsWith("plant grow"))
            {
                int count = ParseIntArg(input, "plant grow", 1);
                CmdPlantGrow(count);
            }
            else if (input.StartsWith("tick set"))
            {
                int ms = ParseIntArg(input, "tick set", -1);
                CmdTickSet(ms);
            }
            else if (input == "tick reset")
            {
                CmdTickReset();
            }
            else if (input == "kill")
            {
                CmdKill();
            }
            else if (input == "godmode")
            {
                CmdGodMode();
            }
            else if (input == "minigame end")
            {
                CmdMinigameEnd();
            }
            else if (input.StartsWith("minigame start"))
            {
                string arg = ParseStringArg(input, "minigame start");
                CmdMinigameStart(arg);
            }
            else
            {
                outputLines.Add("[ERR] Unknown command. Type 'help' for available commands.");
            }
        }
        catch (Exception ex)
        {
            outputLines.Add($"[ERR] {ex.Message}");
        }

        // Trim output
        while (outputLines.Count > 100)
            outputLines.RemoveAt(0);
    }

    private static int ParseIntArg(string input, string prefix, int defaultVal)
    {
        string remainder = input[prefix.Length..].Trim();
        if (remainder.Length == 0) return defaultVal;
        if (int.TryParse(remainder, out int val)) return val;
        throw new Exception($"Invalid number: '{remainder}'");
    }

    private static string ParseStringArg(string input, string prefix)
    {
        string remainder = input[prefix.Length..].Trim();
        if (remainder.Length == 0) throw new Exception("Missing argument.");
        return remainder;
    }

    // ---- Commands ----

    private static void PrintHelp()
    {
        outputLines.Add("  reset all           - Delete save, restart from 0");
        outputLines.Add("  reset inventory     - Clear all seeds from inventory");
        outputLines.Add("  add seed [n]        - Add n random seeds (default 1)");
        outputLines.Add("  add leaf [n]        - Add n leaves (default 1)");
        outputLines.Add("  add essence [n]     - Add n essence (default 1)");
        outputLines.Add("  weather set <type>  - Set weather (Sunny, Cloudy, Rainy, etc.)");
        outputLines.Add("  stage set <n>       - Set current stage");
        outputLines.Add("  world set <type>    - Set world (Terra, Luna, Marte, etc.)");
        outputLines.Add("  plant grow [n]      - Grow plant n times (default 1)");
        outputLines.Add("  tick set <ms>       - Set game tick interval in ms");
        outputLines.Add("  tick reset          - Reset tick to default (1000ms)");
        outputLines.Add("  kill                - Kill the plant instantly");
        outputLines.Add("  godmode             - Toggle god mode (no damage, only growth)");
        outputLines.Add("  minigame start <t>  - Start a minigame (Blackjack, Cerchio, etc.)");
        outputLines.Add("  minigame end        - Stop all active minigames");
    }

    private static void CmdResetAll()
    {
        GameSave.DeleteSaveFile();
        SaveHelper.Delete("inventory.json");
        Inventario.get().seeds.Clear();
        Inventario.get().Save();
        Game.pianta?.Reset();
        if (Game.pianta != null) Game.pianta.Stats = new PlantStats();
        WaterSystem.Current = 100f;
        WaterSystem.Max = 100f;
        SeedUpgradeSystem.SetEssence(0);
        WorldManager.SetCurrentStage(1);
        WorldManager.SetCurrentWorld(WorldType.Terra);
        Game.pianta?.SetNaturalColors(WorldType.Terra);
        WeatherManager.SetWeather(Weather.Sunny);
        GameSave.get().data = new GameSaveData();
        GameSave.get().Save();
        outputLines.Add("[OK] Full reset done. Save deleted, inventory cleared, plant reset.");
    }

    private static void CmdResetInventory()
    {
        int count = Inventario.get().seeds.Count;
        Inventario.get().seeds.Clear();
        Inventario.get().Save();
        outputLines.Add($"[OK] Inventory cleared ({count} seeds removed).");
    }

    private static void CmdAddSeed(int count)
    {
        if (count <= 0) { outputLines.Add("[ERR] Count must be > 0."); return; }

        var types = Enum.GetValues(typeof(SeedType)).Cast<SeedType>().ToArray();
        var rng = new Random();

        for (int i = 0; i < count; i++)
        {
            var type = types[rng.Next(types.Length)];
            var seed = new Seed(type);
            Inventario.get().AddSeed(seed);
        }
        outputLines.Add($"[OK] Added {count} random seed(s).");
    }

    private static void CmdAddLeaf(int count)
    {
        if (count <= 0) { outputLines.Add("[ERR] Count must be > 0."); return; }
        if (Game.pianta == null) { outputLines.Add("[ERR] No plant exists."); return; }
        Game.pianta.Stats.FoglieAccumulate += count;
        outputLines.Add($"[OK] Added {count} leaf/leaves. Total accumulated: {Game.pianta.Stats.FoglieAccumulate}");
    }

    private static void CmdAddEssence(int count)
    {
        if (count <= 0) { outputLines.Add("[ERR] Count must be > 0."); return; }
        SeedUpgradeSystem.SetEssence(SeedUpgradeSystem.Essence + count);
        outputLines.Add($"[OK] Added {count} essence. Total: {SeedUpgradeSystem.Essence}");
    }

    private static void CmdWeatherSet(string arg)
    {
        if (Enum.TryParse<Weather>(arg, ignoreCase: true, out var weather))
        {
            WeatherManager.SetWeather(weather);
            outputLines.Add($"[OK] Weather set to {weather}.");
        }
        else
        {
            string valid = string.Join(", ", Enum.GetNames(typeof(Weather)));
            outputLines.Add($"[ERR] Unknown weather '{arg}'. Valid: {valid}");
        }
    }

    private static void CmdStageSet(int val)
    {
        if (val < 1) { outputLines.Add("[ERR] Stage must be >= 1."); return; }
        WorldManager.SetCurrentStage(val);
        GameSave.get().Save();
        outputLines.Add($"[OK] Stage set to {val}.");
    }

    private static void CmdWorldSet(string arg)
    {
        if (Enum.TryParse<WorldType>(arg, ignoreCase: true, out var world))
        {
            WorldManager.SetCurrentWorld(world);
            Game.pianta?.Reset();
            Game.pianta?.SetNaturalColors(world);
            GameSave.get().Save();
            outputLines.Add($"[OK] World set to {world}.");
        }
        else
        {
            string valid = string.Join(", ", Enum.GetNames(typeof(WorldType)));
            outputLines.Add($"[ERR] Unknown world '{arg}'. Valid: {valid}");
        }
    }

    private static void CmdPlantGrow(int count)
    {
        if (count <= 0) { outputLines.Add("[ERR] Count must be > 0."); return; }
        if (Game.pianta == null) { outputLines.Add("[ERR] No plant exists."); return; }

        for (int i = 0; i < count; i++)
        {
            Game.pianta.Crescita();
        }
        outputLines.Add($"[OK] Plant grew {count} step(s). Height: {Game.pianta.Stats.Altezza:F1}");
    }

    private static void CmdTickSet(int ms)
    {
        if (ms < 10) { outputLines.Add("[ERR] Tick interval must be >= 10ms."); return; }
        if (Game.Timer != null)
        {
            Game.Timer.Interval = ms;
            outputLines.Add($"[OK] Tick interval set to {ms}ms.");
        }
        else
        {
            outputLines.Add("[ERR] Game timer not initialized.");
        }
    }

    private static void CmdTickReset()
    {
        if (Game.Timer != null)
        {
            Game.Timer.Interval = originalTickInterval;
            outputLines.Add($"[OK] Tick interval reset to {originalTickInterval}ms.");
        }
        else
        {
            outputLines.Add("[ERR] Game timer not initialized.");
        }
    }

    private static void CmdKill()
    {
        if (Game.pianta == null) { outputLines.Add("[ERR] No plant exists."); return; }
        Game.pianta.Stats.Salute = 0;
        outputLines.Add("[OK] Plant killed.");
    }

    private static void CmdMinigameStart(string arg)
    {
        if (Enum.TryParse<TipoMinigioco>(arg, ignoreCase: true, out var tipo))
        {
            if (ManagerMinigames.InCorso)
            {
                outputLines.Add("[ERR] A minigame is already running. Use 'minigame end' first.");
                return;
            }
            isOpen = false;
            ManagerMinigames.AvviaInline(tipo);
            outputLines.Add($"[OK] Started minigame: {tipo}");
        }
        else
        {
            string valid = string.Join(", ", Enum.GetNames(typeof(TipoMinigioco)));
            outputLines.Add($"[ERR] Unknown minigame '{arg}'. Valid: {valid}");
        }
    }

    private static void CmdMinigameEnd()
    {
        if (!ManagerMinigames.InCorso)
        {
            outputLines.Add("[ERR] No minigame is running.");
            return;
        }
        ManagerMinigames.FermaMinigiocoAttivo();
        outputLines.Add("[OK] All minigames stopped.");
    }

    private static void CmdGodMode()
    {
        GodMode = !GodMode;
        if (GodMode && Game.pianta != null)
        {
            // Restore stats to full when enabling
            Game.pianta.Stats.Salute = 1f;
            Game.pianta.Stats.Idratazione = 1f;
            Game.pianta.Stats.Ossigeno = 1f;
            Game.pianta.Stats.Metabolismo = 1f;
            Game.pianta.Stats.Infestata = false;
            Game.pianta.Stats.IntensitaInfestazione = 0f;
        }
        outputLines.Add($"[OK] God mode {(GodMode ? "ON" : "OFF")}");
    }
}
