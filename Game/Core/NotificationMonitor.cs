using System;
using Windows.UI.Notifications;

namespace Plants;

public class NotificationMonitor : GameElement
{
    private float checkTimer = 0f;
    private const float CHECK_INTERVAL = 30f; // Controlla ogni 30 secondi

    private bool hasNotifiedLowWater = false;
    private bool hasNotifiedDying = false;
    private bool hasNotifiedParasites = false;
    private bool hasNotifiedWorldTransition = false;

    public NotificationMonitor()
    {
        this.persistent = true;
    }

    public override void Update()
    {
        checkTimer += Raylib_CSharp.Time.GetFrameTime();

        if (checkTimer >= CHECK_INTERVAL)
        {
            checkTimer = 0f;
            CheckPlantStatus();
        }
    }

    private void CheckPlantStatus()
    {
        var stats = Game.pianta.Stats;

        // Notifica acqua bassa
        if (stats.Idratazione < 0.2f && !hasNotifiedLowWater)
        {
            NotificationManager.ShowPlantNeedsWater();
            hasNotifiedLowWater = true;
        }
        else if (stats.Idratazione > 0.4f)
        {
            hasNotifiedLowWater = false;
        }

        // Notifica pianta morente
        if (stats.Salute < 0.15f && !hasNotifiedDying)
        {
            NotificationManager.ShowPlantDying();
            hasNotifiedDying = true;
        }
        else if (stats.Salute > 0.3f)
        {
            hasNotifiedDying = false;
        }

        // Notifica parassiti
        if (stats.Infestata && stats.IntensitaInfestazione > 0.5f && !hasNotifiedParasites)
        {
            NotificationManager.ShowParasiteInfestation();
            hasNotifiedParasites = true;
        }
        else if (!stats.Infestata || stats.IntensitaInfestazione < 0.2f)
        {
            hasNotifiedParasites = false;
        }

        // Notifica transizione mondo
        float maxHeight = stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        if (stats.Altezza >= maxHeight && !hasNotifiedWorldTransition)
        {
            NotificationManager.ShowWorldTransitionReady();
            hasNotifiedWorldTransition = true;
        }
        else if (stats.Altezza < maxHeight * 0.95f)
        {
            hasNotifiedWorldTransition = false;
        }
    }

    public void ResetNotifications()
    {
        hasNotifiedLowWater = false;
        hasNotifiedDying = false;
        hasNotifiedParasites = false;
        hasNotifiedWorldTransition = false;
    }
}