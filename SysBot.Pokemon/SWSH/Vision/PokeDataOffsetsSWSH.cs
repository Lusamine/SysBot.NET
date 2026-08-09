using System.Collections.Generic;

namespace SysBot.Pokemon;

/// <summary>
/// Sword &amp; Shield RAM offsets
/// </summary>
public class PokeDataOffsetsSWSH
{
    public const string SWSHGameVersion = "1.3.2";
    public const string SwordID = "0100ABF008968000";
    public const string ShieldID = "01008DB008C2C000";

    public const uint BoxStartOffset = 0x45075880;
    public const uint CurrentBoxOffset = 0x450C680E;
    public const uint TrainerDataOffset = 0x45068F18;
    public const uint SoftBanUnixTimespanOffset = 0x450C89E8;
    public const uint IsConnectedOffset = 0x30c7cca8;
    public const uint TextSpeedOffset = 0x450690A0;
    public const uint ItemTreasureAddress = 0x45068970;

    // 0 when not in a battle or raid, 0x40 or 0x41 otherwise.
    public const uint InBattleRaidOffsetSW = 0x3F128624;
    public const uint InBattleRaidOffsetSH = 0x3F128626;

    // Pokémon Encounter Offsets
    public const uint WildPokemonOffset = 0x8FEA3648;
    public const uint RaidPokemonOffset = 0x886A95B8;
    public const uint LegendaryPokemonOffset = 0x886BC348;
    public const uint LastSpeciesSpawned = 0x800AA58;
    public const uint LastFormSpawned = 0x800AA5A;

    // Fishing offset - arbitrary offsets, there may be better ones.
    public const uint FishingOffsetSH = 0x08073330;
    public const uint FishingOffsetSW = 0x01D62BBD;

    /* Route 5 Daycare */
    public const uint DayCare_Route5_Step_Counter = 0x4511F99C;
    public const uint DayCare_Route5_Egg_Is_Ready = 0x4511F9A8;

    // Main RNG Offset
    public const uint SWSHMainRNGOffset = 0x4C2AAC18;

    // Dex Recommendation Block
    public const uint DexRecOffset = 0x45072B18;

    // Max Lair Offsets
    public static IReadOnlyList<long> MaxLairPokemonRNGPointer { get; } = [0x28F4060, 0x238, 0x2AB8];
    public const uint MaxLairPenaltyWarnOffset = 0x50B06FC0;
    public const uint MaxLairPenaltyCountOffset = 0x50B12710;

    // Curry Offsets
    public static IReadOnlyList<long> CurrySpawnPointer { get; } = [0x296C030, 0x60, 0x40, 0xB0, 0x98, 0x10, 0x0];
    // advances for party size. Optimized for party size of 2 -- do the rest yourself.
    public static readonly int[] CurryAdvances = [0, 1167, 1169, 1171, 1173, 1175, 1177];

    public const int BoxFormatSlotSize = 0x158;
    public const int TrainerDataLength = 0x110;

    #region ScreenDetection
    // Stable overworld detection. Value is 1 on overworld and 0 otherwise.
    public IReadOnlyList<long> OverworldPointer { get; } = [0x2636678, 0xC0, 0x80];

    // For detecting when we're on the in-battle menu, so we can flee.
    public const uint BattleMenuOffset = 0x8398A470;
    #endregion
}
