using System;
using System.Collections.Generic;
using System.Linq;

namespace Plants;

public static class ManagerMinigames
{
    private static Dictionary<TipoMinigioco, MinigiocoBase> minigiochi = new();
    private static Room room_minigioco;
    private static bool inCorso = false;

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
    }

    public static Room GetRoom()
    {
        return room_minigioco;
    }

    public static void Avvia(TipoMinigioco tipo)
    {
        if (inCorso) return;
        if (!minigiochi.ContainsKey(tipo)) return;

        inCorso = true;

        // Prima attiva la room (questo riattiva TUTTI gli elementi nella room)
        room_minigioco.SetActiveRoom();

        // Poi disattiva e resetta tutti i minigiochi
        foreach (var mg in minigiochi.Values)
            mg.Ferma();

        // Infine avvia solo quello scelto
        minigiochi[tipo].Avvia();
    }

    public static void AvviaCasuale()
    {
        if (inCorso) return;

        var tipi = minigiochi.Keys.ToList();
        var tipo = tipi[RandomHelper.Int(0, tipi.Count)];
        Avvia(tipo);
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
