using System;

namespace Plants;

/// <summary>
/// Sistema di eventi per la pianta - notifica cambiamenti di stato
/// </summary>
public class PlantEventSystem
{
    // Eventi per notifiche
    public event Action OnLowWater;
    public event Action OnCriticalHealth;
    public event Action OnParasiteInfestation;
    public event Action OnWorldTransitionReady;
    public event Action OnTemperatureDanger;

    // Stato precedente per rilevare cambiamenti
    private bool wasLowWater = false;
    private bool wasCritical = false;
    private bool wasInfested = false;
    private bool wasReadyForTransition = false;
    private bool wasTemperatureDanger = false;

    private GameLogicPianta gameLogic;

    public PlantEventSystem(GameLogicPianta logic)
    {
        gameLogic = logic;
    }

    /// <summary>
    /// Controlla lo stato e genera eventi se necessario
    /// </summary>
    public void CheckAndFireEvents()
    {
        if (gameLogic.stats == null) return;

        // Evento acqua bassa
        bool isLowWater = gameLogic.IsDisidratata;
        if (isLowWater && !wasLowWater)
        {
            OnLowWater?.Invoke();
        }
        wasLowWater = isLowWater;

        // Evento salute critica
        bool isCritical = gameLogic.IsCritica;
        if (isCritical && !wasCritical)
        {
            OnCriticalHealth?.Invoke();
        }
        wasCritical = isCritical;

        // Evento parassiti
        bool isInfested = gameLogic.stats.Infestata && gameLogic.stats.IntensitaInfestazione > 0.5f;
        if (isInfested && !wasInfested)
        {
            OnParasiteInfestation?.Invoke();
        }
        wasInfested = isInfested;

        // Evento transizione mondo
        float maxHeight = gameLogic.stats.AltezzaMassima * WorldManager.GetCurrentModifiers().LimitMultiplier;
        bool isReady = gameLogic.stats.Altezza >= maxHeight;
        if (isReady && !wasReadyForTransition)
        {
            OnWorldTransitionReady?.Invoke();
        }
        wasReadyForTransition = isReady;

        // Evento temperatura pericolosa
        bool isTempDanger = gameLogic.IsGelida || gameLogic.IsTorrida;
        if (isTempDanger && !wasTemperatureDanger)
        {
            OnTemperatureDanger?.Invoke();
        }
        wasTemperatureDanger = isTempDanger;
    }

    /// <summary>
    /// Reset dello stato (utile dopo azioni correttive)
    /// </summary>
    public void ResetState(int state)
    {
        switch (state)
        {
            case 0: 
                wasLowWater = false;
                break;
            case 1:
                wasCritical = false;
                break;
            case 2: 
                wasInfested = false;
                break;
            case 3:
                wasReadyForTransition = false;
                break;
            case 4: 
                wasTemperatureDanger = false;
                break;
        }
    }
}