using System;

namespace Plants;

public class NotificationMonitor : GameElement
{
    private float checkTimer = 0f;
    private const float CHECK_INTERVAL = 30f; // Controlla ogni 30 secondi

    // Flag per evitare spam di notifiche
    private bool hasNotifiedLowWater = false;
    private bool hasNotifiedDying = false;
    private bool hasNotifiedParasites = false;
    private bool hasNotifiedWorldTransition = false;
    private bool hasNotifiedTemperature = false;

    // Timer per reinviare notifiche se l'utente non risponde
    private float lowWaterTimer = 0f;
    private float dyingTimer = 0f;
    private float parasitesTimer = 0f;
    private float temperatureTimer = 0f;
    private const float NOTIFICATION_TIMEOUT = 3f; // 5 minuti (300 secondi)

    public NotificationMonitor()
    {
        this.persistent = true;

        // Registra gli eventi del sistema di eventi della pianta
        if (Game.pianta?.proprieta?.EventSystem != null)
        {
            RegisterPlantEvents();
        }
    }

    /// <summary>
    /// Registra i listener per gli eventi della pianta
    /// </summary>
    private void RegisterPlantEvents()
    {
        var eventSystem = Game.pianta.proprieta.EventSystem;

        eventSystem.OnLowWater += () =>
        {
            // Invia notifica solo se l'app non è in primo piano
            if (!WindowStateHelper.IsGameWindowFocused())
            {
                if (!hasNotifiedLowWater)
                {
                    NotificationManager.ShowPlantNeedsWater();
                    hasNotifiedLowWater = true;
                    Console.WriteLine("[Event] Notifica: Acqua bassa (app in background)");
                }
            }
            else
            {
                Console.WriteLine("[Event] Acqua bassa rilevata (app attiva, notifica non inviata)");
                Game.pianta.proprieta.EventSystem.ResetState(0);
            }
        };

        eventSystem.OnCriticalHealth += () =>
        {
            if (!WindowStateHelper.IsGameWindowFocused())
            {
                if (!hasNotifiedDying)
                {
                    NotificationManager.ShowPlantDying();
                    hasNotifiedDying = true;
                    Console.WriteLine("[Event] Notifica: Salute critica (app in background)");
                }
            }
            else
            {
                Console.WriteLine("[Event] Salute critica rilevata (app attiva, notifica non inviata)");
                Game.pianta.proprieta.EventSystem.ResetState(1);
            }
        };

        eventSystem.OnParasiteInfestation += () =>
        {
            if (!WindowStateHelper.IsGameWindowFocused())
            {
                if (!hasNotifiedParasites)
                {
                    NotificationManager.ShowParasiteInfestation();
                    hasNotifiedParasites = true;
                    Console.WriteLine("[Event] Notifica: Parassiti (app in background)");
                }
            }
            else
            {
                Console.WriteLine("[Event] Parassiti rilevati (app attiva, notifica non inviata)");
                Game.pianta.proprieta.EventSystem.ResetState(2);
            }
        };

        eventSystem.OnWorldTransitionReady += () =>
        {
            // Per la transizione mondo inviamo sempre la notifica
            if (!hasNotifiedWorldTransition)
            {
                NotificationManager.ShowWorldTransitionReady();
                hasNotifiedWorldTransition = true;
                Console.WriteLine("[Event] Notifica: Transizione mondo pronta");
                Game.pianta.proprieta.EventSystem.ResetState(3);
            }
        };

        eventSystem.OnTemperatureDanger += () =>
        {
            if (!WindowStateHelper.IsGameWindowFocused())
            {
                if (!hasNotifiedTemperature)
                {
                    NotificationManager.ShowTemperatureDanger();
                    hasNotifiedTemperature = true;
                    Console.WriteLine("[Event] Notifica: Temperatura pericolosa (app in background)");
                }
            }
            else
            {
                Console.WriteLine("[Event] Temperatura pericolosa rilevata (app attiva, notifica non inviata)");
                Game.pianta.proprieta.EventSystem.ResetState(4);
            }
        };
    }

    public override void Update()
    {
        float deltaTime = Raylib_CSharp.Time.GetFrameTime();
        checkTimer += deltaTime;

        if (checkTimer >= CHECK_INTERVAL)
        {
            checkTimer = 0f;
            CheckPlantStatus();
        }

        // Controlla gli eventi ogni frame
        if (Game.pianta?.proprieta?.EventSystem != null)
        {
            Game.pianta.proprieta.EventSystem.CheckAndFireEvents();
        }

        // Aggiorna i timer delle notifiche attive
        UpdateNotificationTimers(deltaTime);
    }

    /// <summary>
    /// Aggiorna i timer e reinvia le notifiche se necessario
    /// </summary>
    private void UpdateNotificationTimers(float deltaTime)
    {
        var stats = Game.pianta.Stats;

        // Reinvia notifiche solo se l'app è in background
        bool isBackground = !WindowStateHelper.IsGameWindowFocused();

        // Timer acqua bassa
        if (hasNotifiedLowWater && stats.Idratazione < 0.2f)
        {
            lowWaterTimer += deltaTime;
            if (lowWaterTimer >= NOTIFICATION_TIMEOUT && isBackground)
            {
                NotificationManager.ShowPlantNeedsWater();
                lowWaterTimer = 0f;
                Console.WriteLine("[Timeout] Reinvio notifica: Acqua bassa");
            }
        }
        else
        {
            lowWaterTimer = 0f;
        }

        // Timer salute critica
        if (hasNotifiedDying && stats.Salute < 0.2f)
        {
            dyingTimer += deltaTime;
            if (dyingTimer >= NOTIFICATION_TIMEOUT && isBackground)
            {
                NotificationManager.ShowPlantDying();
                dyingTimer = 0f;
                Console.WriteLine("[Timeout] Reinvio notifica: Salute critica");
            }
        }
        else
        {
            dyingTimer = 0f;
        }

        // Timer parassiti
        if (hasNotifiedParasites && stats.Infestata && stats.IntensitaInfestazione > 0.5f)
        {
            parasitesTimer += deltaTime;
            if (parasitesTimer >= NOTIFICATION_TIMEOUT && isBackground)
            {
                NotificationManager.ShowParasiteInfestation();
                parasitesTimer = 0f;
                Console.WriteLine("[Timeout] Reinvio notifica: Parassiti");
            }
        }
        else
        {
            parasitesTimer = 0f;
        }

        // Timer temperatura
        if (hasNotifiedTemperature && (Game.pianta.proprieta.IsGelida || Game.pianta.proprieta.IsTorrida))
        {
            temperatureTimer += deltaTime;
            if (temperatureTimer >= NOTIFICATION_TIMEOUT && isBackground)
            {
                NotificationManager.ShowTemperatureDanger();
                temperatureTimer = 0f;
                Console.WriteLine("[Timeout] Reinvio notifica: Temperatura");
            }
        }
        else
        {
            temperatureTimer = 0f;
        }
    }

    /// <summary>
    /// Controllo periodico per reset dei flag di notifica
    /// </summary>
    private void CheckPlantStatus()
    {
        var stats = Game.pianta.Stats;

        // Reset flag se la situazione è migliorata
        if (stats.Idratazione > 0.4f)
        {
            hasNotifiedLowWater = false;
        }

        if (stats.Salute > 0.3f)
        {
            hasNotifiedDying = false;
        }

        if (!stats.Infestata || stats.IntensitaInfestazione < 0.2f)
        {
            hasNotifiedParasites = false;
        }

        float maxHeight = stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        if (stats.Altezza < maxHeight * 0.95f)
        {
            hasNotifiedWorldTransition = false;
        }

        if (!Game.pianta.proprieta.IsGelida && !Game.pianta.proprieta.IsTorrida)
        {
            hasNotifiedTemperature = false;
        }
    }

    /// <summary>
    /// Reset manuale di tutte le notifiche
    /// </summary>
    public void ResetNotifications()
    {
        hasNotifiedLowWater = false;
        hasNotifiedDying = false;
        hasNotifiedParasites = false;
        hasNotifiedWorldTransition = false;
        hasNotifiedTemperature = false;

        // Reset anche i timer
        lowWaterTimer = 0f;
        dyingTimer = 0f;
        parasitesTimer = 0f;
        temperatureTimer = 0f;
    }
}