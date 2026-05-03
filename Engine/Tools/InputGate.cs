namespace Plants;

/// <summary>
/// Gate per il mouse: consente agli elementi GUI in primo piano (cassetto, popup, topbar)
/// di "consumare" il click del frame, così che gli elementi sotto non lo processino.
/// Resettato a inizio frame in Rendering.cs. Gli elementi sono aggiornati in ordine di depth
/// crescente (sopra prima), quindi l'overlay vede e consuma per primo.
/// </summary>
public static class InputGate
{
    public static bool MouseConsumed { get; private set; }

    public static void Reset() { MouseConsumed = false; }
    public static void ConsumeMouse() { MouseConsumed = true; }
}
