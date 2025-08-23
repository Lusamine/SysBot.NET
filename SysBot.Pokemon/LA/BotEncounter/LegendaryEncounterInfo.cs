namespace SysBot.Pokemon;
using PKHeX.Core;
using System.Collections.Generic;
using System.Linq;
using static EncounterBotOWLCheckRNGLA.AreaID;

public class LegendEncounterInfo(OWLegendary species, LegendResetMode mode, EncounterBotOWLCheckRNGLA.AreaID area, ulong hash, uint start)
{
    public OWLegendary Species { get; } = species;
    public LegendResetMode Mode { get; } = mode;
    public EncounterBotOWLCheckRNGLA.AreaID Area { get; } = area;
    public ulong SpawnerHash { get; } = hash;
    public uint StartIndex { get; } = start;

    private static readonly LegendEncounterInfo[] Legendaries =
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
        new(OWLegendary.Phione,    LegendResetMode.Cave,      Coastlands,                0x0, 420), // No hash since we track 12 spawners.

        new(OWLegendary.Uxie,      LegendResetMode.Cave,      Icelands,   0x5A67A8230FAF4B95, 305),
        new(OWLegendary.Mesprit,   LegendResetMode.Cave,      Fieldlands, 0x375CE0719E93B5CF, 315),
        new(OWLegendary.Azelf,     LegendResetMode.Cave,      Mirelands,  0x33DB175E98E892F7, 405),
    ];

    private static readonly ulong[][][] PhioneSpawnerHashes =
    [
        // Hashes used before Manaphy is caught
        [
            [0x7C9E6D810B343908, 0x162F766DB48FC6D8, 0xE1CBACBBA2D63665], // 0 Phione caught
            [0xF9CAB353536235F1, 0x8096D8BE845B33A9],                     // 1 Phione caught
            [0x8882A996F7887C95],                                         // 2 Phione caught
        ],
        // Hashes used after Manaphy is caught
        [
            [0xDA2E3F6002EC2DB0, 0xACD5411BD6DC1877, 0xEB7C479F58A6F641], // 0 Phione caught
            [0x77265D26C92C43A7, 0x59BA4BDD623CE4BC],                     // 1 Phione caught
            [0x7FF822CF35A35BF4],                                         // 2 Phione caught
        ]
    ];

    public static LegendResetMode GetLegendaryMode(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.Mode ?? LegendResetMode.None;
    }

    public static EncounterBotOWLCheckRNGLA.AreaID GetLegendaryArea(OWLegendary legendary)
    {
        return Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.Area ?? None;
    }

    public static List<ulong> GetLegendarySpawnerHash(OWLegendary legendary)
    {
        return [Legendaries.FirstOrDefault(entry => entry.Species == legendary)?.SpawnerHash ?? 0];
    }

    // Fetches the array of spawner hashes for Phione based on whether Manaphy has been caught and how many Phione have been caught.
    public static List<ulong>? GetPhioneSpawnerHashes(bool manaphy_caught, int phione_caught, bool include_all_layers)
    {
        if (phione_caught is < 0 or > 3)
            return null;

        int max_phione_caught = include_all_layers ? 2 : phione_caught;
        var manaphy_set = manaphy_caught ? 1 : 0;

        var result = new List<ulong>();
        for (int i = phione_caught; i <= max_phione_caught; i++)
            result.AddRange(PhioneSpawnerHashes[manaphy_set][i]);

        return result;
    }

    // Finds how many Phione need to be caught to reach a specific spawner.
    public static int GetNumberPhioneToCatch(ulong spawner)
    {
        for (var i = 0; i <= 2; i++)
        {
            if (PhioneSpawnerHashes[0][i].Contains(spawner))
                return i;
            if (PhioneSpawnerHashes[1][i].Contains(spawner))
                return i;
        }
        return -1;
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
