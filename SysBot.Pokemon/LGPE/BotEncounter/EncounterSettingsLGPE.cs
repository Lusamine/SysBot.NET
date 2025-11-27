using PKHeX.Core;
using SysBot.Base;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;

namespace SysBot.Pokemon
{
    public class EncounterSettingsLGPE : IBotStateSettings, ICountSettings
    {
        private const string Counts = nameof(Counts);
        private const string EncounterLGPE = nameof(EncounterLGPE);
        public override string ToString() => "Encounter LGPE Bot Settings";

        [Category(EncounterLGPE), Description("The method by which the bot will encounter Pokémon.")]
        public EncounterModeLGPE EncounteringType { get; set; } = EncounterModeLGPE.StaticLGPE;

        [Category(EncounterLGPE), Description("When enabled, the bot will continue after finding a suitable match.")]
        public ContinueAfterMatch ContinueAfterMatch { get; set; } = ContinueAfterMatch.StopExit;

        [Category(EncounterLGPE), Description("Select the catch combo species.")]
        public Species CatchComboSpecies { get; set; }

        [Category(EncounterLGPE), Description("Set the catch combo length. Leave as \"0\" to disable.")]
        public int CatchComboLength { get; set; } = 50;

        [Category(EncounterLGPE), Description("Max Lure is activated to speed up RNG advancements.")]
        public bool SetMaxLureAdvancements { get; set; }

        [Category(EncounterLGPE), Description("Max Lure is activated for encounters. Pokémon encountered will be max level +1.")]
        public bool SetMaxLureEncounter { get; set; }

        [Category(EncounterLGPE), Description("Sets the Fortune Teller nature. Leave as \"Random\" to disable.")]
        public Nature FortuneTellerNature { get; set; } = Nature.Random;

        [Category(EncounterLGPE), Description("Set the maximum number of RNG advances to check for a legendary bird to spawn.")]
        public int MaxBirdWatchAdvances { get; set; } = 64;

        [Category(EncounterLGPE), Description("Select whether to minimize the game when a legendary bird spawns. If disabled, the routine will continue if stop conditions are not met.")]
        public bool PauseOnBirdMatch { get; set; } = true;

        [Category(EncounterLGPE), Description("Extra time in milliseconds to wait after entering the Pokémon Center.")]
        public int ExtraTimeEnterPMC { get; set; }

        [Category(EncounterLGPE), Description("Extra time in milliseconds to wait after leaving the Pokémon Center.")]
        public int ExtraTimeExitPMC { get; set; }

        [Category(EncounterLGPE), Description("Sets the number of gift or trade Pokémon to check before resetting the game.")]
        public int GiftTradePokemonCount { get; set; } = 1;

        [Category(EncounterLGPE), Description("The style to display the global RNG state.")]
        public DisplaySeedMode DisplaySeedMode { get; set; } = DisplaySeedMode.Bit64;

        [Category(EncounterLGPE), Description("Interval in milliseconds for the monitor to check the Main RNG state.")]
        public int MonitorRefreshRate { get; set; } = 500;

        [Category(EncounterLGPE), Description("Maximum total advances before the RNG monitor pauses the game by clicking X. Set to 0 to disable.")]
        public int MaxTotalAdvances { get; set; }

        [Category(EncounterLGPE), Description("When enabled, the screen will be turned off during normal bot loop operation to save power.")]
        public bool ScreenOff { get; set; }

        private int _completedResets;
        private int _completedRadar;
        private int _completedBirds;

        [Category(Counts), Description("Pokémon found through game resets.")]
        public int CompletedResets
        {
            get => _completedResets;
            set => _completedResets = value;
        }

        [Category(Counts), Description("Pokémon found through overworld radar scanning.")]
        public int CompletedRadar
        {
            get => _completedRadar;
            set => _completedRadar = value;
        }

        [Category(Counts), Description("Legendary birds found through overworld radar scanning.")]
        public int CompletedBirds
        {
            get => _completedBirds;
            set => _completedBirds = value;
        }

        [Category(Counts), Description("When enabled, the counts will be emitted when a status check is requested.")]
        public bool EmitCountsOnStatusCheck { get; set; }

        public int AddCompletedResets() => Interlocked.Increment(ref _completedResets);
        public int AddCompletedRadar() => Interlocked.Increment(ref _completedRadar);
        public int AddCompletedBirds() => Interlocked.Increment(ref _completedBirds);

        public IEnumerable<string> GetNonZeroCounts()
        {
            if (!EmitCountsOnStatusCheck)
                yield break;
            if (CompletedResets != 0)
                yield return $"Total Game Resets: {_completedResets}";
            if (CompletedRadar != 0)
                yield return $"Total Game Resets: {_completedRadar}";
        }
    }
}
