using System.Collections.Generic;

namespace SysBot.Pokemon;

public class PokeDataOffsetsBS_SP : BasePokeDataOffsetsBS
{
    public override IReadOnlyList<long> BoxStartPokemonPointer         { get; } = [0x4E7BE98, 0xB8, 0x10, 0xA0, 0x20, 0x20, 0x20];

    public override IReadOnlyList<long> SceneIDPointer                 { get; } = [0x4E70C28, 0xB8, 0x18];

    public override IReadOnlyList<long> MyStatusTrainerPointer         { get; } = [0x4E7BE98, 0xB8, 0x10, 0xE0, 0x0];
    public override IReadOnlyList<long> MyStatusTIDPointer             { get; } = [0x4E7BE98, 0xB8, 0x10, 0xE8];
    public override IReadOnlyList<long> ConfigTextSpeedPointer         { get; } = [0x4E7BE98, 0xB8, 0x10, 0xA8];
    public override IReadOnlyList<long> ConfigLanguagePointer          { get; } = [0x4E7BE98, 0xB8, 0x10, 0xAC];

    // Main RNG state
    public override IReadOnlyList<long> MainRNGPointer                 { get; } = [0x4FD43D0, 0x0];

    // ZoneID Pointer
    public override IReadOnlyList<long> ZoneIDPointer                  { get; } = [0x4E7BE98, 0xB8, 0x10, 0x40];
}
