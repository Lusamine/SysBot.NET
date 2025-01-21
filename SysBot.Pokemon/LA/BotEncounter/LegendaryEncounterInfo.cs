namespace SysBot.Pokemon;
using PKHeX.Core;
using System.Linq;
using static EncounterBotOWLCheckRNGLA.AreaID;

public class LegendEncounterInfo(OWLegendary species, LegendResetMode mode, EncounterBotOWLCheckRNGLA.AreaID area, ulong hash, uint start)
{
    public OWLegendary Species { get; } = species;
    public LegendResetMode Mode { get; } = mode;
    public EncounterBotOWLCheckRNGLA.AreaID Area { get; } = area;
    public ulong SpawnerHash { get; } = hash;
    public uint StartIndex { get; } = start;

    public static readonly LegendEncounterInfo[] Legendaries =
    [
        new(OWLegendary.Tornadus,  LegendResetMode.Wandering, Icelands,   0xB8B7D33AB95F7A11, 305),
        new(OWLegendary.Thundurus, LegendResetMode.Wandering, Coastlands, 0x08398514506FBE25, 420),
        new(OWLegendary.Landorus,  LegendResetMode.Wandering, Fieldlands, 0x88AF9BCFDD5FCD8F, 315),
        new(OWLegendary.Enamorus,  LegendResetMode.Wandering, Mirelands,  0xA468ADF5964CCE65, 405),

        new(OWLegendary.Cresselia, LegendResetMode.Wandering, Highlands,  0x80E30B44446F80BE, 270),
        new(OWLegendary.Darkrai,   LegendResetMode.Wandering, Highlands,  0x14044B0D5D36E4B6, 270),
        new(OWLegendary.Shaymin,   LegendResetMode.Wandering, Fieldlands, 0xCDE0EAB0B0192256, 315),

        new(OWLegendary.Heatran,   LegendResetMode.Cave,      Coastlands, 0xDA1BB574FA53D58C, 420),
        new(OWLegendary.Manaphy,   LegendResetMode.Cave,      Coastlands, 0x97D85B3BB18FD0AB, 420),

        new(OWLegendary.Uxie,      LegendResetMode.Cave,      Icelands,   0x5A67A8230FAF4B95, 305),
        new(OWLegendary.Mesprit,   LegendResetMode.Cave,      Fieldlands, 0x375CE0719E93B5CF, 315),
        new(OWLegendary.Azelf,     LegendResetMode.Cave,      Mirelands,  0x33DB175E98E892F7, 405),
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

    public static uint GetStartIndex(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.StartIndex ?? 512;
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
