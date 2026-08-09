using System.Collections.Generic;

namespace SysBot.Pokemon;

/// <summary>
/// Pokémon Legends: Z-A RAM offsets
/// </summary>
public class PokeDataOffsetsLZA
{
    public const string LZAGameVersion = "2.0.2";
    public const string LegendsZAID = "0100F43008C44000";

    public IReadOnlyList<long> BoxStartPokemonPointer           { get; } = [0x610A710, 0xB0, 0x978, 0x0];
    public IReadOnlyList<long> TextSpeedPointer                 { get; } = [0x610A710, 0xD8, 0x40];
    public IReadOnlyList<long> MyStatusPointer                  { get; } = [0x610A710, 0xA0, 0x40];
    public IReadOnlyList<long> PartyPointer                     { get; } = [0x610A710, 0x18, 0x1B0, 0xF0, 0x50, 0x30, 0x0];
    public IReadOnlyList<long> CurrentBoxPointer                { get; } = [0x610A710, 0xA8, 0x596];

    // Main offsets
    public const uint OverworldOffset = 0x610C858;
    public const uint MenuOffset      = 0x612DA80;
    public const uint ConnectedOffset = 0x6133458;

    public const uint InBattleOffset   = 0x610B9C0;
    public const uint InWildZoneOffset = 0x610C8A8;

    public const int BoxFormatSlotSize = 0x148;
}
