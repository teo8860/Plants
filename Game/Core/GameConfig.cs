using System;
using Raylib_CSharp.Interact;
using Raylib_CSharp.Windowing;

namespace Plants;

public class GameConfigData
{
    public int UiScale = 1;
    public bool StartHidden = false;
    public bool CloseOnX = false;
}

public class GameConfig
{
    public const string FileName = "options.json";

    private static GameConfig instance = null;
    public static GameConfig get()
    {
        if (instance == null)
            instance = new GameConfig();
        return instance;
    }

    public GameConfigData data = new GameConfigData();

    private GameConfig()
    {
        Load();
    }

    public void Load()
    {
        var loaded = SaveHelper.Load<GameConfigData>(FileName);
        if (loaded != null)
            data = loaded;

        if (data.UiScale < 1 || data.UiScale > GameProperties.MaxUiScaleLevel)
            data.UiScale = 1;
    }

    public void Save()
    {
        SaveHelper.Save(FileName, data);
    }

    public int UiScale
    {
        get => data.UiScale;
        set
        {
            int v = Math.Clamp(value, 1, GameProperties.MaxUiScaleLevel);
            if (data.UiScale != v)
            {
                data.UiScale = v;
                Save();
                ApplyScaleLive();
            }
        }
    }

    /// <summary>
    /// Applica la scala corrente alla finestra gia' inizializzata (resize runtime + mouse scale).
    /// Nessun riavvio richiesto.
    /// </summary>
    public static void ApplyScaleLive()
    {
        GameProperties.uiScale = get().data.UiScale;
        int physW = GameProperties.physicalWindowWidth;
        int physH = GameProperties.physicalWindowHeight;

        Window.SetSize(physW, physH);
        Window.SetPosition(Window.GetMonitorWidth(0) - physW - 20, Window.GetMonitorHeight(0) - physH - 50);

        float inv = 1f / GameProperties.uiScaleMultiplier;
        Input.SetMouseScale(inv, inv);
    }

    public bool StartHidden
    {
        get => data.StartHidden;
        set
        {
            if (data.StartHidden != value)
            {
                data.StartHidden = value;
                Save();
            }
        }
    }

    public bool CloseOnX
    {
        get => data.CloseOnX;
        set
        {
            if (data.CloseOnX != value)
            {
                data.CloseOnX = value;
                Save();
            }
        }
    }
}
