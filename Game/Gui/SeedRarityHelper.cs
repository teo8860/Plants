using Raylib_CSharp.Colors;

namespace Plants;

public static class SeedRarityHelper
{
    public static Color GetColor(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune      => new Color(200, 200, 200, 255),
        SeedRarity.NonComune   => new Color(80,  200, 80,  255),
        SeedRarity.Raro        => new Color(80,  150, 255, 255),
        SeedRarity.Epico       => new Color(180, 80,  255, 255),
        SeedRarity.Leggendario => new Color(255, 180, 50,  255),
        SeedRarity.Mitico      => new Color(255, 80,  150, 255),
        _                      => Color.White
    };

    public static string GetName(SeedRarity rarity) => rarity switch
    {
        SeedRarity.Comune      => "Comune",
        SeedRarity.NonComune   => "Non Comune",
        SeedRarity.Raro        => "Raro",
        SeedRarity.Epico       => "Epico",
        SeedRarity.Leggendario => "Leggendario",
        SeedRarity.Mitico      => "Mitico",
        _                      => "???"
    };
}
