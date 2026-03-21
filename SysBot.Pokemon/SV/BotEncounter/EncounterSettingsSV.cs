using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SysBot.Base;

namespace SysBot.Pokemon
{
    public class EncounterSettingsSV : IBotStateSettings, ICountSettings
    {
        private const string Counts = nameof(Counts);
        private const string Encounter = nameof(Encounter);
        public override string ToString() => "Encounter Bot SV Settings";

        [Category(Encounter), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(Encounter), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        private int _completedOutbreaks;

        [Category(Counts), Description("Outbreaks checked.")]
        public int CompletedOutbreaks
        {
            get => _completedOutbreaks;
            set => _completedOutbreaks = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedOutbreaks(int count) => Interlocked.Add(ref _completedOutbreaks, count);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedOutbreaks != 0)
                yield return $"Outbreaks Checked: {CompletedOutbreaks}";
        }
    }
}
