using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class EncounterSettingsRNGBS : IBotStateSettings, ICountSettings
    {
        private const string Counts = nameof(Counts);
        private const string EncounterRNGBS = nameof(EncounterRNGBS);
        public override string ToString() => "Encounter RNG BS Bot Settings";

        [Category(EncounterRNGBS), Description("The style to export the global RNG state.")]
        public DisplaySeedMode DisplaySeedMode { get; set; } = DisplaySeedMode.Bit32;

        [Category(EncounterRNGBS), Description("Number of advances the bot will make for TID RNG in BDSP.")]
        public int MaxTIDAdvances { get; set; }

        [Category(EncounterRNGBS), Description("Set this to \"True\" if you already have a target, and advancing was previously interrupted.")]
        public bool TIDAdvanceOnly { get; set; } = false;

        [Category(EncounterRNGBS), Description("Interval in milliseconds for the monitor to check the Main RNG state.")]
        public int MonitorRefreshRate { get; set; } = 500;

        [Category(EncounterRNGBS), Description("Maximum total advances before the RNG monitor pauses the game by clicking HOME. Set to 0 to disable.")]
        public int MaxTotalAdvances { get; set; }

        [Category(EncounterRNGBS), Description("Configures sys-botbase mainLoopSleepTime in ms for DexFlip. Default is 50.")]
        public int DexFlipMainLoopSleepTime { get; set; } = 39;

        [Category(EncounterRNGBS), Description("Configures delay for each joystick movement for DexFlip.")]
        public int DexFlipStickSetTime { get; set; } = 40;

        [Category(EncounterRNGBS), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(EncounterRNGBS), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
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
