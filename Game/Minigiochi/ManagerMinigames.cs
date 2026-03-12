using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Plants;

public static class ManagerMinigames
{
    private static Dictionary<TipoMinigioco, MinigiocoBase> minigiochi = new();
    private static Room room_minigioco;
    private static bool inCorso = false;
    private static Process? processoMinigioco = null;

    public static bool InCorso => inCorso;

    public static void Init()
    {
        room_minigioco = new Room(false);

        var cerchio = GameElement.Create<MinigiocoCerchio>(0, room_minigioco);
        minigiochi[TipoMinigioco.Cerchio] = cerchio;

        var tieni = GameElement.Create<MinigiocoTieni>(0, room_minigioco);
        minigiochi[TipoMinigioco.Tieni] = tieni;

        var resta = GameElement.Create<MinigiocoResta>(0, room_minigioco);
        minigiochi[TipoMinigioco.Resta] = resta;

        var semi = GameElement.Create<MinigiocoSemi>(0, room_minigioco);
        minigiochi[TipoMinigioco.Semi] = semi;

        var treni = GameElement.Create<MinigiocoTreni>(0, room_minigioco);
        minigiochi[TipoMinigioco.Treni] = treni;
    }

    /// <summary>
    /// Inizializza solo un minigioco specifico per la modalità standalone.
    /// </summary>
    public static void InitStandalone(TipoMinigioco tipo)
    {
        room_minigioco = new Room(true); // room attiva subito

        MinigiocoBase minigioco = tipo switch
        {
            TipoMinigioco.Cerchio => GameElement.Create<MinigiocoCerchio>(0, room_minigioco),
            TipoMinigioco.Tieni => GameElement.Create<MinigiocoTieni>(0, room_minigioco),
            TipoMinigioco.Resta => GameElement.Create<MinigiocoResta>(0, room_minigioco),
            TipoMinigioco.Semi => GameElement.Create<MinigiocoSemi>(0, room_minigioco),
            TipoMinigioco.Treni => GameElement.Create<MinigiocoTreni>(0, room_minigioco),
            _ => throw new ArgumentException($"Tipo minigioco sconosciuto: {tipo}")
        };

        minigiochi[tipo] = minigioco;
        minigioco.Avvia();
    }

    public static Room GetRoom()
    {
        return room_minigioco;
    }

    /// <summary>
    /// Avvia un processo separato per il minigioco.
    /// </summary>
    public static void AvviaProcesso(TipoMinigioco tipo)
    {
        if (inCorso) return;

        inCorso = true;

        string exePath = Environment.ProcessPath ?? Process.GetCurrentProcess().MainModule?.FileName
            ?? throw new InvalidOperationException("Impossibile trovare il percorso dell'eseguibile");

        processoMinigioco = new Process();
        processoMinigioco.StartInfo.FileName = exePath;
        processoMinigioco.StartInfo.Arguments = $"--minigioco {tipo}";
        processoMinigioco.EnableRaisingEvents = true;
        processoMinigioco.Exited += (s, e) =>
        {
            // Il processo minigioco è terminato, controlla i risultati
            var risultato = MinigameResult.Load();
            if (risultato != null && risultato.FoglieGuadagnate > 0)
            {
                Game.pianta.Stats.FoglieAccumulate += risultato.FoglieGuadagnate;
            }
            inCorso = false;
            processoMinigioco = null;
        };

        processoMinigioco.Start();
    }

    public static void OnMinigiocoFinito()
    {
        inCorso = false;
    }

    public static List<TipoMinigioco> GetTipiDisponibili()
    {
        return minigiochi.Keys.ToList();
    }
}
