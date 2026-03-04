using PKHeX.Core;
using System.Linq;
using static PKHeX.Core.GameVersion;
using static PKHeX.Core.LanguageID;

namespace SysBot.Pokemon;

public class LanguageVersionOffsetsFRLG(LanguageID Language, GameVersion Version, string TitleID, uint CurrentSeedOffset, uint WildPokemonOffset, uint PartyStartOffset, uint OverworldOffset)
{
    public LanguageID Language { get; } = Language;
    public GameVersion Version { get; } = Version;
    public string TitleID { get; } = TitleID;
    public uint CurrentSeedOffset { get; } = CurrentSeedOffset;
    public uint WildPokemonOffset { get; } = WildPokemonOffset;
    public uint PartyStartOffset { get; } = PartyStartOffset;  // add 0x64 per slot
    public uint OverworldOffset { get; } = OverworldOffset; // FF on overworld/battle, 0 on load game menus, 1 when menu is open

    private static readonly LanguageVersionOffsetsFRLG[] FRLGVersionOffsets =
    [
        new(Japanese, FR, "01006FA0233F8000", 0xBD68D230, 0x120BF88, 0x120C1E0, 0x1222B54),
        new(Japanese, LG, "0100F1E0233FA000", 0xBD68D230, 0x120BF88, 0x120C1E0, 0x1222B54),
        new(English,  FR, "0100554023408000", 0xBD68D2D0, 0x120C028, 0x120C280, 0x1222BDC),
        new(English,  LG, "010034D02340E000", 0xBD68D2D0, 0x120C028, 0x120C280, 0x1222BDC),
        new(French,   FR, "01004B3023412000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(French,   LG, "010087C02342E000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(Italian,  FR, "010092302342A000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(Italian,  LG, "01005C7023432000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(German,   FR, "01007F8023416000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(German,   LG, "0100FD6023430000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(Spanish,  FR, "0100EB702342C000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
        new(Spanish,  LG, "01002B5023434000", 0xBD68D220, 0x120C028, 0x120C280, 0x1222BDC),
    ];

    public static LanguageID GetLanguageFromTitleID(string titleID)
    {
        // Matches the provided titleID from LanguageVersionOffsetsFRLG and returns the matching LanguageID.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.TitleID == titleID)?.Language ?? None;
    }

    public static GameVersion GetGameVersionFromTitleID(string titleID)
    {
        // Matches the provided titleID from LanguageVersionOffsetsFRLG and returns the matching GameVersion.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.TitleID == titleID)?.Version ?? Any;
    }

    public static uint GetCurrentSeedOffsetFromTitleID(string titleID)
    {
        // Matches the provided titleID from LanguageVersionOffsetsFRLG and returns the matching CurrentSeedOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.TitleID == titleID)?.CurrentSeedOffset ?? 0;
    }

    public static uint GetCurrentSeedOffsetFromLanguageAndVersion(LanguageID language, GameVersion version)
    {
        // Matches the provided language and version from LanguageVersionOffsetsFRLG and returns the matching CurrentSeedOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.Language == language && entry.Version == version)?.CurrentSeedOffset ?? 0;
    }

    public static uint GetWildPokemonOffsetFromLanguageAndVersion(LanguageID language, GameVersion version)
    {
        // Matches the provided language and version from LanguageVersionOffsetsFRLG and returns the matching WildPokemonOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.Language == language && entry.Version == version)?.WildPokemonOffset ?? 0;
    }

    public static uint GetPartyStartOffsetFromTitleID(string titleID)
    {
        // Matches the provided titleID from LanguageVersionOffsetsFRLG and returns the matching PartyStartOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.TitleID == titleID)?.PartyStartOffset ?? 0;
    }

    public static uint GetPartyStartOffsetFromLanguageAndVersion(LanguageID language, GameVersion version)
    {
        // Matches the provided language and version from LanguageVersionOffsetsFRLG and returns the matching PartyStartOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.Language == language && entry.Version == version)?.PartyStartOffset ?? 0;
    }

    public static uint GetOverworldOffsetFromTitleID(string titleID)
    {
        // Matches the provided titleID from LanguageVersionOffsetsFRLG and returns the matching OverworldOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.TitleID == titleID)?.OverworldOffset ?? 0;
    }

    public static uint GetOverworldOffsetFromLanguageAndVersion(LanguageID language, GameVersion version)
    {
        // Matches the provided language and version from LanguageVersionOffsetsFRLG and returns the matching OverworldOffset.
        return FRLGVersionOffsets.FirstOrDefault(entry => entry.Language == language && entry.Version == version)?.OverworldOffset ?? 0;
    }

    public static bool IsTitleIDValidFRLG(string titleID)
    {
        // Checks if the provided titleID exists in the LanguageVersionOffsetsFRLG array.
        return FRLGVersionOffsets.Any(entry => entry.TitleID == titleID);
    }
}
