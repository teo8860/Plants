using Microsoft.Toolkit.Uwp.Notifications;
using Raylib_CSharp.Windowing;
using System;

namespace Plants;

public static class NotificationManager
{
    private const string APP_ID = "Plants.Game";

    public static void Initialize()
    {
        // Registra l'app per le notifiche
        ToastNotificationManagerCompat.OnActivated += OnNotificationActivated;

        // Inizializza il gestore delle azioni
        NotificationActionHandler.Initialize();
    }

    private static void OnNotificationActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        Console.WriteLine($"Notifica cliccata con argomento: {e.Argument}");

        // Estrae l'azione dagli argomenti
        var args = ToastArguments.Parse(e.Argument);

        if (args.Contains("action"))
        {
            string action = args["action"];

            // Esegui l'azione tramite l'handler
            NotificationActionHandler.HandleAction(action);
        }
        else
        {
            // Se non c'è azione specifica, apri solo il gioco
            if (Window.IsReady())
            {
                Window.ClearState(ConfigFlags.HiddenWindow);
            }
        }
    }

    public static void ShowPlantNeedsWater()
    {
        new ToastContentBuilder()
            .AddText("La tua pianta ha sete! 🌱")
            .AddText("L'idratazione è sotto il 20%")
            .AddButton(new ToastButton()
                .SetContent("Annaffia")
                .AddArgument("action", "water"))
            .AddButton(new ToastButton()
                .SetContent("Apri gioco")
                .AddArgument("action", "open"))
            .Show();
    }

    public static void ShowPlantDying()
    {
        new ToastContentBuilder()
            .AddText("⚠️ ATTENZIONE!")
            .AddText("La tua pianta sta morendo!")
            .AddAttributionText($"Salute: {Game.pianta.Stats.Salute:P0}")
            .AddButton(new ToastButton()
                .SetContent("Salva la pianta")
                .AddArgument("action", "rescue"))
            .AddButton(new ToastButton()
                .SetContent("Apri gioco")
                .AddArgument("action", "open"))
            .Show();
    }

    public static void ShowWorldTransitionReady()
    {
        var nextWorld = WorldManager.GetNextWorld(WorldManager.GetCurrentWorld());

        new ToastContentBuilder()
            .AddText("🚀 Nuovo mondo disponibile!")
            .AddText($"Puoi viaggiare verso {WorldManager.GetWorldName(nextWorld)}")
            .AddButton(new ToastButton()
                .SetContent("Viaggia")
                .AddArgument("action", "travel"))
            .AddButton(new ToastButton()
                .SetContent("Apri gioco")
                .AddArgument("action", "open"))
            .Show();
    }

    public static void ShowParasiteInfestation()
    {
        new ToastContentBuilder()
            .AddText("🐛 Parassiti rilevati!")
            .AddText("La tua pianta è infestata")
            .AddAttributionText($"Intensità: {Game.pianta.Stats.IntensitaInfestazione:P0}")
            .AddButton(new ToastButton()
                .SetContent("Cura")
                .AddArgument("action", "cure"))
            .AddButton(new ToastButton()
                .SetContent("Apri gioco")
                .AddArgument("action", "open"))
            .Show();
    }

    public static void ShowTemperatureDanger()
    {
        string tempStatus = Game.pianta.proprieta.IsGelida ? "GELIDA" : "TORRIDA";

        new ToastContentBuilder()
            .AddText($"🌡️ Temperatura {tempStatus}!")
            .AddText($"La temperatura è pericolosa: {Game.pianta.Stats.Temperatura:F1}°C")
            .AddButton(new ToastButton()
                .SetContent("Controlla")
                .AddArgument("action", "open"))
            .Show();
    }

    public static void Cleanup()
    {
        ToastNotificationManagerCompat.Uninstall();
    }
}