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
    }

    private static void OnNotificationActivated(ToastNotificationActivatedEventArgsCompat e)
    {
        // Gestisci il click sulla notifica
        Console.WriteLine($"Notifica cliccata: {e.Argument}");

        // Riporta la finestra in primo piano
        if (Window.IsReady())
        {
            Window.ClearState(ConfigFlags.HiddenWindow);
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
        ;
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
            .Show();
        ;
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
            .Show();
        ;
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
            .Show();
        ;
    }

    public static void Cleanup()
    {
        ToastNotificationManagerCompat.Uninstall();
    }
}