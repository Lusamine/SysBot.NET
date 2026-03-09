using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon;

public class EncounterSettingsFRLG : IBotStateSettings, ICountSettings
{
    private const string Counts = nameof(Counts);
    private const string EncounterFRLG = nameof(EncounterFRLG);
    public override string ToString() => "Encounter Bot FRLG Settings";

    [Category(EncounterFRLG), Description("The method used by Reset bot to encounter Pokémon.")]
    public EncounterModeFRLG EncounteringType { get; set; } = EncounterModeFRLG.StaticFRLG;

    [Category(EncounterFRLG), Description("The Pokémon to purchase from the Game Corner.")]
    public GameCornerPrizeFRLG GameCornerPrizeToPurchase { get; set; } = GameCornerPrizeFRLG.Abra;

    [Category(EncounterFRLG), Description("The number of Pokémon to purchase from the Game Corner each reset.")]
    public int GameCornerNumberToPurchase { get; set; } = 1;

    [Category(EncounterFRLG), Description("Adds up to this many milliseconds after a soft reset and before starting the game. Increases the number of potential Pokémon encountered due to FRLG's initial seeding.")]
    public int RandomTimeSoftReset { get; set; } = 5000;

    [Category(EncounterFRLG), Description("Adds up to this many milliseconds before an encounter. Increases the number of potential Pokémon encountered due to FRLG's initial seeding.")]
    public int RandomTimeBeforeEncounter { get; set; } = 5000;

    [Category(EncounterFRLG), Description("Logs the initial seed in reset bots to track variability.")]
    public bool LogInitialSeed { get; set; }

    [Category(EncounterFRLG), Description("Interval in milliseconds for the monitor to check the Main RNG state.")]
    public int MonitorRefreshRate { get; set; }

    [Category(EncounterFRLG), Description("Maximum total advances before the RNG monitor pauses the game by clicking X. Set to 0 to disable.")]
    public int MaxTotalAdvances { get; set; }

    [Category(EncounterFRLG), Description("When enabled, the bot will continue after finding a suitable match.")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

    [Category(EncounterFRLG), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; }

    private int _completedWild;
    private int _completedLegend;
    private int _completedGift;

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

    [Category(Counts), Description("Encountered Gift Pokémon")]
    public int CompletedGifts
    {
        get => _completedGift;
        set => _completedGift = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);
    public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);
    public int AddCompletedGifts() => Interlocked.Increment(ref _completedGift);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedEncounters != 0)
            yield return $"Wild/Gift Encounters: {CompletedEncounters}";
        if (CompletedLegends != 0)
            yield return $"Legendary Encounters: {CompletedLegends}";
        if (CompletedGifts != 0)
            yield return $"Gifts Received: {CompletedGifts}";
    }
}
