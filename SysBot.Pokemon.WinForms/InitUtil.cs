using PKHeX.Core;

namespace SysBot.Pokemon.WinForms;

public static class InitUtil
{
    public static void InitializeStubs(ProgramMode mode)
    {
        var sav = GetFakeSaveFile(mode);
    }

    private static SaveFile GetFakeSaveFile(ProgramMode mode) => mode switch
    {
        ProgramMode.SWSH => new SAV8SWSH(),
        ProgramMode.BDSP => new SAV8BS(),
        ProgramMode.LA   => new SAV8LA(),
        ProgramMode.SV   => new SAV9SV(),
        ProgramMode.LZA  => new SAV9ZA(),
        ProgramMode.LGPE => new SAV7b(),
        ProgramMode.FRLG => new SAV3FRLG(),
        _                => throw new System.ArgumentOutOfRangeException(nameof(mode)),
    };
}
