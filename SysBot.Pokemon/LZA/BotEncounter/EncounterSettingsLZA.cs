using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon;

public class EncounterSettingsLZA : IBotStateSettings, ICountSettings
{
    private const string Counts = nameof(Counts);
    private const string EncounterLZA = nameof(EncounterLZA);
    private const string Settings = nameof(Settings);
    public override string ToString() => "Encounter Bot LZA Settings";

    [Category(EncounterLZA), Description("The method by which the bot will encounter Pokémon.")]
    public EncounterModeLZA EncounteringType { get; set; } = EncounterModeLZA.WildZone10LZA;

    [Category(EncounterLZA), Description("Number of times to loop for Virizion or Terrakion.")]
    public int LegendLoops { get; set; } = 40;

    [Category(EncounterLZA), Description("When enabled, the bot will continue after finding a suitable match.")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

    [Category(EncounterLZA), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; }

    private int _completedWild;
    private int _completedLegend;

    [Category(Counts), Description("Encountered Wild Pokémon")]
    public int CompletedEncounters
    {
        get => _completedWild;
        set => _completedWild = value;
    }

    [Category(Counts), Description("Encountered Legendary Pokémon")]
    public int CompletedLegends
    {
        get => _completedLegend;
        set => _completedLegend = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);
    public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedEncounters != 0)
            yield return $"Wild Encounters: {CompletedEncounters}";
        if (CompletedLegends != 0)
            yield return $"Legendary Encounters: {CompletedLegends}";
    }
}
