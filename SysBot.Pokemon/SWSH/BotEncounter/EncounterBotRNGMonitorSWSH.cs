using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotRNGMonitorSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        private int TotalAdvances;

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            var (s0, s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterSWSH.DisplaySeedMode);

            // Attempt to copy initial state to clipboard.
            CopyToClipboard(output);

            Log("Initial RNG state copied to the clipboard.");
            Log($"Start: {output}");

            while (!token.IsCancellationRequested)
            {
                var wait = Hub.Config.EncounterSWSH.MonitorRefreshRate;
                if (Hub.Config.EncounterSWSH.BellsAndWhistles)
                {
                    // We need to ring the bell until the wait is over.
                    while (wait >= 0)
                    {
                        await Click(LSTICK, 0_100, token).ConfigureAwait(false);
                        wait -= 0_100;
                    }
                }
                else
                {
                    // If we aren't ringing the bell, we can just wait the time out.
                    await Task.Delay(wait, token).ConfigureAwait(false);
                }

                var (_s0, _s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);

                // Only update if it changed.
                if (_s0 == s0 && _s1 == s1)
                    continue;

                output = GetSeedMonitorOutput(_s0, _s1, Hub.Config.EncounterSWSH.DisplaySeedMode);
                var passed = GetAdvancesPassed(s0, s1, _s0, _s1);
                TotalAdvances += passed;
                Log($"{output} - Advances: {TotalAdvances} | {passed}");

                // Store the state for the next pass.
                s0 = _s0;
                s1 = _s1;

                var maxAdvance = Hub.Config.EncounterSWSH.MaxTotalAdvances;
                if (maxAdvance != 0 && TotalAdvances >= maxAdvance)
                {
                    Log($"Hitting X to pause the game. Max total advances is {maxAdvance} and {TotalAdvances} advances have passed.");
                    await Click(X, 2_000, token).ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}
