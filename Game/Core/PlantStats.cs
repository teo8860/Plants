using System;
using System.Collections.Generic;

namespace Plants;

public enum SeedType
{
    Normale,        // Seme base senza bonus
    Poderoso,       // +Vitalità
    Fluviale,       // +Idratazione (cala più lentamente)
    Glaciale,       // +Resistenza al freddo
    Magmatico,      // +Resistenza al caldo
    Puro,           // +Resistenza ai parassiti
    Florido,        // +Vegetazione (più foglie)
    Rapido          // +Metabolismo (cresce più veloce)
}

public enum SeedRarity
{
    Comune,         // 45%
    NonComune,      // 25%
    Raro,           // 15%
    Esotico,        // 8%
    Epico,          // 4%
    Leggendario,    // 2%
    Mitico          // 1%
}

public struct SeedBonus
{
    public float Vitalita;              // Bonus alla vita massima
    public float Idratazione;           // Moltiplicatore consumo acqua (< 1 = consuma meno)
    public float ResistenzaFreddo;      // Riduzione danni da freddo
    public float ResistenzaCaldo;       // Riduzione danni da caldo
    public float ResistenzaParassiti;   // Riduzione danni/probabilità parassiti
    public float Vegetazione;           // Moltiplicatore foglie
    public float Metabolismo;           // Moltiplicatore velocità crescita

    public static SeedBonus Default => new()
    {
        Vitalita = 1.0f,
        Idratazione = 1.0f,
        ResistenzaFreddo = 0.0f,
        ResistenzaCaldo = 0.0f,
        ResistenzaParassiti = 0.0f,
        Vegetazione = 1.0f,
        Metabolismo = 1.0f
    };
}

public static class SeedDataType
{
    private static readonly Dictionary<SeedType, SeedBonus> _bonuses = new()
    {
        { SeedType.Normale, SeedBonus.Default },

        { SeedType.Poderoso, new SeedBonus
        {
            Vitalita = 1.5f,
            Idratazione = 1.0f,
            ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = 0.0f,
            ResistenzaParassiti = 0.0f,
            Vegetazione = 1.0f,
            Metabolismo = 0.9f  
        }},

        { SeedType.Fluviale, new SeedBonus
        {
            Vitalita = 1.0f,
            Idratazione = 0.5f,  
            ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = -0.1f,  
            ResistenzaParassiti = 0.0f,
            Vegetazione = 1.0f,
            Metabolismo = 1.0f
        }},

        { SeedType.Glaciale, new SeedBonus
        {
            Vitalita = 1.0f,
            Idratazione = 1.0f,
            ResistenzaFreddo = 0.8f,  
            ResistenzaCaldo = -0.3f,  
            ResistenzaParassiti = 0.0f,
            Vegetazione = 0.9f,
            Metabolismo = 0.9f
        }},

        { SeedType.Magmatico, new SeedBonus
        {
            Vitalita = 1.0f,
            Idratazione = 1.3f,  
            ResistenzaFreddo = -0.3f,  
            ResistenzaCaldo = 0.8f,  
            ResistenzaParassiti = 0.0f,
            Vegetazione = 0.9f,
            Metabolismo = 1.0f
        }},

        { SeedType.Puro, new SeedBonus
        {
            Vitalita = 0.9f, 
            Idratazione = 1.0f,
            ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = 0.0f,
            ResistenzaParassiti = 0.9f,  
            Vegetazione = 1.0f,
            Metabolismo = 1.0f
        }},

        { SeedType.Florido, new SeedBonus
        {
            Vitalita = 1.0f,
            Idratazione = 1.2f,  
            ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = 0.0f,
            ResistenzaParassiti = -0.1f,  
            Vegetazione = 2.0f,  
            Metabolismo = 1.0f
        }},

        { SeedType.Rapido, new SeedBonus
        {
            Vitalita = 0.8f,  
            Idratazione = 1.4f,  
            ResistenzaFreddo = 0.0f,
            ResistenzaCaldo = 0.0f,
            ResistenzaParassiti = 0.0f,
            Vegetazione = 1.0f,
            Metabolismo = 1.8f  
        }}
    };

    public static SeedBonus GetBonus(SeedType type) =>
        _bonuses.TryGetValue(type, out var bonus) ? bonus : SeedBonus.Default;

    //se vogliamo mettere le info a schermo
    public static string GetName(SeedType type) => type switch
    {
        SeedType.Normale => "Seme Normale",
        SeedType.Poderoso => "Seme Poderoso",
        SeedType.Fluviale => "Seme Fluviale",
        SeedType.Glaciale => "Seme Glaciale",
        SeedType.Magmatico => "Seme Magmatico",
        SeedType.Puro => "Seme Puro",
        SeedType.Florido => "Seme Florido",
        SeedType.Rapido => "Seme Rapido",
        _ => "Seme Sconosciuto"
    };

    public static string GetDescription(SeedType type) => type switch
    {
        SeedType.Normale => "Un seme comune senza particolari proprietà.",
        SeedType.Poderoso => "Produce piante robuste con maggiore vitalità.",
        SeedType.Fluviale => "Adatto ad ambienti umidi, consuma meno acqua.",
        SeedType.Glaciale => "Resiste alle basse temperature ma soffre il caldo.",
        SeedType.Magmatico => "Prospera nel caldo ma richiede più acqua.",
        SeedType.Puro => "Naturalmente resistente ai parassiti.",
        SeedType.Florido => "Produce abbondante fogliame decorativo.",
        SeedType.Rapido => "Crescita accelerata ma richiede più cure.",
        _ => "Proprietà sconosciute."
    };
}

public class PlantStats
{

    public float Salute = 1.0f;
    public float Idratazione = 0.0f;
    public float Ossigeno = 1.0f;
    public float Metabolismo = 1.0f;

    public float Temperatura = 20.0f;
    public float ResistenzaFreddo = 0.0f;
    public float ResistenzaCaldo = 0.0f;
    public float ResistenzaParassiti = 0.0f;

    public int FoglieBase = 50;
    public int FoglieAttuali = 50;
    public float DropRateFoglie = 0.01f;

    public float Altezza = 0.0f;
    public float AltezzaMassima = 100.0f;

    public bool Infestata = false;
    public float IntensitaInfestazione = 0.0f;

}