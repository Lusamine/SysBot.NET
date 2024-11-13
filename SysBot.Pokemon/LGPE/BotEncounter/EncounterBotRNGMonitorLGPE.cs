using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    // This is for a specific RNG state that controls item spawning on the ground and other less useful
    // random processes like battle animations, follower Pokémon interactions, and field Pokémon movement.
    // If other RNG states need monitoring, this may be renamed in the future.
    public sealed class EncounterBotRNGMonitorLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : EncounterBotLGPE(cfg, hub)
    {
        private int TotalAdvances;
        private ulong GeneralRNGOffset;

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            await InitializeGeneralRNGPointer(token).ConfigureAwait(false);
            Log($"{GeneralRNGOffset:x8}");

            var (s0, s1) = await GetGlobalRNGState(GeneralRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterLGPE.DisplaySeedMode);

            // Attempt to copy initial state to clipboard.
            CopyToClipboard(output);

            Log("Initial RNG state copied to the clipboard.");
            Log($"Start: {output}");

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Hub.Config.EncounterLGPE.MonitorRefreshRate, token).ConfigureAwait(false);

                var (_s0, _s1) = await GetGlobalRNGState(GeneralRNGOffset, false, token).ConfigureAwait(false);

                // Only update if it changed.
                if (_s0 == s0 && _s1 == s1)
                    continue;

                output = GetSeedMonitorOutput(_s0, _s1, Hub.Config.EncounterLGPE.DisplaySeedMode);
                var passed = GetAdvancesPassed(s0, s1, _s0, _s1);
                TotalAdvances += passed;
                Log($"{output} - Advances: {TotalAdvances} | {passed}");

                // Store the state for the next pass.
                s0 = _s0;
                s1 = _s1;

                var maxAdvance = Hub.Config.EncounterLGPE.MaxTotalAdvances;
                if (maxAdvance != 0 && TotalAdvances >= maxAdvance)
                {
                    Log($"Hitting X to pause the game. Max total advances is {maxAdvance} and {TotalAdvances} advances have passed.");
                    await Click(X, 2_000, token).ConfigureAwait(false);
                    return;
                }
            }
        }
        private async Task InitializeGeneralRNGPointer(CancellationToken token)
        {
            await SetMainLoopSleepTime(50, token).ConfigureAwait(false);
            bool valid = false;
            while (!valid)
                (valid, GeneralRNGOffset) = await ValidatePointerAll(LGPEGeneralRNGPointer, token).ConfigureAwait(false);
            await SetMainLoopSleepTime(35, token).ConfigureAwait(false);
            await Task.Delay(1_700, token).ConfigureAwait(false);
        }
    }
}
