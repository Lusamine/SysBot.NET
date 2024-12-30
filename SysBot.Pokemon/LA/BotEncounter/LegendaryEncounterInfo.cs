namespace SysBot.Pokemon;
using PKHeX.Core;
using System.Linq;
using static EncounterBotOWLCheckRNGLA.AreaID;

public class LegendEncounterInfo(OWLegendary species, LegendResetMode mode, EncounterBotOWLCheckRNGLA.AreaID area, ulong hash)
{
    public OWLegendary Species { get; } = species;
    public LegendResetMode Mode { get; } = mode;
    public EncounterBotOWLCheckRNGLA.AreaID Area { get; } = area;
    public ulong SpawnerHash { get; } = hash;

    public static readonly LegendEncounterInfo[] Legendaries =
    [
        new(OWLegendary.Tornadus,  LegendResetMode.Wandering, Icelands,   0xB8B7D33AB95F7A11),
        new(OWLegendary.Thundurus, LegendResetMode.Wandering, Coastlands, 0x08398514506FBE25),
        new(OWLegendary.Landorus,  LegendResetMode.Wandering, Fieldlands, 0x88AF9BCFDD5FCD8F),
        new(OWLegendary.Enamorus,  LegendResetMode.Wandering, Mirelands,  0xA468ADF5964CCE65),

        new(OWLegendary.Cresselia, LegendResetMode.Wandering, Highlands,  0x80E30B44446F80BE),
        new(OWLegendary.Darkrai,   LegendResetMode.Wandering, Highlands,  0x14044B0D5D36E4B6),
        new(OWLegendary.Shaymin,   LegendResetMode.Wandering, Fieldlands, 0xCDE0EAB0B0192256),

        new(OWLegendary.Heatran,   LegendResetMode.Cave,      Coastlands, 0xDA1BB574FA53D58C),
        new(OWLegendary.Manaphy,   LegendResetMode.Cave,      Coastlands, 0x97D85B3BB18FD0AB),

        new(OWLegendary.Uxie,      LegendResetMode.Cave,      Icelands,   0x5A67A8230FAF4B95),
        new(OWLegendary.Mesprit,   LegendResetMode.Cave,      Fieldlands, 0x375CE0719E93B5CF),
        new(OWLegendary.Azelf,     LegendResetMode.Cave,      Mirelands,  0x33DB175E98E892F7),
    ];

    public static LegendResetMode GetLegendaryMode(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.Mode ?? LegendResetMode.None;
    }

    public static EncounterBotOWLCheckRNGLA.AreaID GetLegendaryArea(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.Area ?? None;
    }

    public static ulong GetLegendarySpawnerHash(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.SpawnerHash ?? 0;
    }
}

public enum OWLegendary
{
    None = Species.None,
    Uxie = Species.Uxie,
    Mesprit = Species.Mesprit,
    Azelf = Species.Azelf,
    Heatran = Species.Heatran,
    Cresselia = Species.Cresselia,
    Phione = Species.Phione,
    Manaphy = Species.Manaphy,
    Darkrai = Species.Darkrai,
    Shaymin = Species.Shaymin,
    Tornadus = Species.Tornadus,
    Thundurus = Species.Thundurus,
    Landorus = Species.Landorus,
    Enamorus = Species.Enamorus,
}

public enum LegendResetMode
{
    None = 0,
    Wandering = 1,
    Cave = 2,
}
