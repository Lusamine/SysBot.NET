using SysBot.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;

namespace SysBot.Pokemon;

public class EncounterSettings : IBotStateSettings, ICountSettings
{
    private const string Counts = nameof(Counts);
    private const string Encounter = nameof(Encounter);
    private const string Settings = nameof(Settings);
    public override string ToString() => "Encounter Bot SWSH Settings";

    [Category(Encounter), Description("The method used by the Line and Reset bots to encounter Pokémon.")]
    public EncounterMode EncounteringType { get; set; } = EncounterMode.VerticalLine;

    [Category(Settings)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public FossilSettings Fossil { get; set; } = new();

    [Category(Settings)]
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public CurrySettings Curry { get; set; } = new();

    [Category(Encounter), Description("The note for the Pokémon to select when starting a Dynamax Adventure. This can be 1-4.")]
    public int MaxLairNoteToPick { get; set; } = 1;

    [Category(Encounter), Description("The rental Pokémon to pick when a match is found. This can be 1-3.")]
    public int MaxLairRentalToPick { get; set; } = 1;

    [Category(Encounter), Description("When enabled, the bot will continue after finding a suitable match.")]
    public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

    [Category(Encounter), Description("The style to export the global RNG state.")]
    public DisplaySeedMode DisplaySeedMode { get; set; } = DisplaySeedMode.Bit32;

    [Category(Encounter), Description("Interval in milliseconds for the monitor to check the Main RNG state.")]
    public int MonitorRefreshRate { get; set; } = 500;

    [Category(Encounter), Description("Maximum total advances before the RNG monitor pauses the game by clicking X. Set to 0 to disable.")]
    public int MaxTotalAdvances { get; set; }

    [Category(Encounter), Description("If enabled, bot will continuously press L-stick to ring the bell or whistle to advance the RNG.")]
    public bool BellsAndWhistles { get; set; }

    [Category(Encounter), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
    public bool ScreenOff { get; set; }

    private int _completedWild;
    private int _completedLegend;
    private int _completedEggs;
    private int _completedFossils;

    [Category(Counts), Description("Encountered Wild/Gift Pokémon")]
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

    [Category(Counts), Description("Eggs Retrieved")]
    public int CompletedEggs
    {
        get => _completedEggs;
        set => _completedEggs = value;
    }

    [Category(Counts), Description("Fossil Pokémon Revived")]
    public int CompletedFossils
    {
        get => _completedFossils;
        set => _completedFossils = value;
    }

    [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
    public bool EmitCountsOnStatusCheck { get; set; }

    public int AddCompletedEncounters() => Interlocked.Increment(ref _completedWild);
    public int AddCompletedLegends() => Interlocked.Increment(ref _completedLegend);
    public int AddCompletedEggs() => Interlocked.Increment(ref _completedEggs);
    public int AddCompletedFossils() => Interlocked.Increment(ref _completedFossils);

    public IEnumerable<string> GetNonZeroCounts()
    {
        if (!EmitCountsOnStatusCheck)
            yield break;
        if (CompletedEncounters != 0)
            yield return $"Wild/Gift Encounters: {CompletedEncounters}";
        if (CompletedLegends != 0)
            yield return $"Legendary Encounters: {CompletedLegends}";
        if (CompletedEggs != 0)
            yield return $"Eggs Received: {CompletedEggs}";
        if (CompletedFossils != 0)
            yield return $"Completed Fossils: {CompletedFossils}";
    }

    public static void ReadTargetSlots(PokeTradeHubConfig hub, out IReadOnlyList<(int, int)> slots)
    {
        slots = hub.EncounterSWSH.Curry.CurryTargetSlots.Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => s.Split('-', StringSplitOptions.RemoveEmptyEntries))
            .Select(s => (int.Parse(s[0]), int.Parse(s[1])))
            .ToList();
    }

    public static bool IsSlotMatch(IReadOnlyList<(int, int)> slots, int randroll) => slots.Any(s => s.Item1 <= randroll && s.Item2 >= randroll);
    public static bool IsSlotMatch(IReadOnlyList<(int, int)> slots, float randroll) => slots.Any(s => s.Item1 <= randroll && s.Item2 >= randroll);
}
