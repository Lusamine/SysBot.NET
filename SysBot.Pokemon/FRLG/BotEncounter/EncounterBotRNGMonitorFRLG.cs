using PKHeX.Core;
using SysBot.Base;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.LanguageVersionOffsetsFRLG;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotRNGMonitorFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        private uint InitialRNGSeed;
        private uint CurrentRNGOffset;
        private uint PrevRNGState;
        private uint TotalAdvances;
        private string TitleID = string.Empty;

        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time lets us more finely read advances.
            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.mainLoopSleepTime, 15, UseCRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            CurrentRNGOffset = GetCurrentSeedOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
            TitleID = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);

            // First check the Box pointer to see if the RNG has been initialized.
            if (!await IsBoxPointerLoaded(token).ConfigureAwait(false))
            {
                Log("RNG is not yet initialized. Waiting for game to start.");
                while (!await IsBoxPointerLoaded(token).ConfigureAwait(false))
                    await Task.Delay(0_300, token).ConfigureAwait(false);
            }

            // If we get to here, the box pointer is loaded, so we can start monitoring the RNG.
            InitialRNGSeed = await GetInitialRNGState(false, token).ConfigureAwait(false);

            // Attempt to copy initial state to clipboard.
            CopyToClipboard(InitialRNGSeed.ToString());
            Log("Initial RNG state copied to the clipboard.");
            Log($"Start: {InitialRNGSeed:x4}");
            PrevRNGState = InitialRNGSeed;

            while (!token.IsCancellationRequested)
            {
                await Task.Delay(Hub.Config.EncounterFRLG.MonitorRefreshRate, token).ConfigureAwait(false);

                uint current_state = await GetCurrentRNGState(CurrentRNGOffset, false, token).ConfigureAwait(false);

                // Only update if it changed.
                if (current_state == PrevRNGState)
                    continue;

                TotalAdvances = LCRNG.GetDistance(InitialRNGSeed, current_state);
                var passed = LCRNG.GetDistance(PrevRNGState, current_state);
                Log($"{current_state:x8} - Advances: {TotalAdvances} | {passed}");

                // Store the state for the next pass.
                PrevRNGState = current_state;

                var maxAdvance = Hub.Config.EncounterLA.MaxTotalAdvances;
                if (maxAdvance != 0 && TotalAdvances >= maxAdvance)
                {
                    Log($"Hitting HOME to pause the game. Max total advances is {maxAdvance} and {TotalAdvances} advances have passed.");
                    await Click(HOME, 2_000, token).ConfigureAwait(false);
                    return;
                }
            }
        }

        private async Task<bool> IsBoxPointerLoaded(CancellationToken token)
        {
            var offset = await GetBoxStartOffset(TitleID, token).ConfigureAwait(false);
            return offset != 0;
        }
    }
}
