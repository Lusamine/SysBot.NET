namespace SysBot.Pokemon;

/// <summary>
/// FRLG RAM offsets
/// </summary>
public class PokeDataOffsetsFRLG
{
    public const string FRLGGameVersion = "1.0.0";

    // Offsets relative to heap.
    public const uint InitialSeed = 0x1208000;

    // The pointers to save blocks are relative to each language's CurrentSeed offset.
    public const uint LargeShift = 0x8;
    public const uint SmallShift = 0xC;
    public const uint BoxStartShift = 0x10;

    // For detecting when we're in a battle.
    public const uint InBattleOffset = 0x6D9C91; // 02 in battle, 01 outside battle
    public const uint BattleMenuOffset = 0;

    public const int BoxFormatSlotSize = 0x50;
}
