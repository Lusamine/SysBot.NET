using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class EncounterLASettings : IBotStateSettings, ICountSettings
    {
        private const string Counts = nameof(Counts);
        private const string EncounterLA = nameof(EncounterLA);
        public override string ToString() => "Encounter LA Bot Settings";

        [Category(EncounterLA), Description("The style to export the global RNG state.")]
        public DisplaySeedMode DisplaySeedMode { get; set; } = DisplaySeedMode.Bit32;

        [Category(EncounterLA), Description("Interval in milliseconds for the monitor to check the Main RNG state.")]
        public int MonitorRefreshRate { get; set; } = 500;

        [Category(EncounterLA), Description("Maximum total advances before the RNG monitor pauses the game by clicking HOME. Set to 0 to disable.")]
        public int MaxTotalAdvances { get; set; }

        [Category(EncounterLA), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(EncounterLA), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        private int _completedResets;

        [Category(Counts), Description("Total game resets.")]
        public int CompletedResets
        {
            get => _completedResets;
            set => _completedResets = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedResets() => Interlocked.Increment(ref _completedResets);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedResets != 0)
                yield return $"Total Game Resets: {_completedResets}";
        }
    }
}
