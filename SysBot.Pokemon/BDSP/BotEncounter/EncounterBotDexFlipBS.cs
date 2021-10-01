using PKHeX.Core;
using SysBot.Base;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotDexFlipBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : EncounterBotBS(cfg, hub)
    {
        private ulong MainRNGOffset;

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time can allow for faster sending of commands.
            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.mainLoopSleepTime, Hub.Config.EncounterRNGBS.DexFlipMainLoopSleepTime, UseCRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            MainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);
            ulong s0 = 0, s1 = 0;

            while (!token.IsCancellationRequested)
            {
                // Performance degrades after a few minutes, so reset the Dex.
                for (int i = 0; i < 2500; i++)
                {
                    // Check on the global RNG state every 750 passes.
                    if (i % 750 == 0)
                        (s0, s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);

                    await SetStick(LEFT, -30000, 0, Hub.Config.EncounterRNGBS.DexFlipStickSetTime, token).ConfigureAwait(false);
                    await SetStick(LEFT, 30000, 0, Hub.Config.EncounterRNGBS.DexFlipStickSetTime, token).ConfigureAwait(false);

                    if (i % 750 == 0 && await CheckIfRNGStatePaused(s0, s1, token).ConfigureAwait(false))
                        return;
                }

                // Check before resetting the dex to avoid undoing our pause.
                if (await CheckIfRNGStatePaused(s0, s1, token).ConfigureAwait(false))
                    return;

                await ResetStick(token).ConfigureAwait(false);
                await Click(B, 1_000, token).ConfigureAwait(false);
                Log("Resetting the Pokédex to handle degraded performance.");
                await Click(A, 1_000, token).ConfigureAwait(false);
            }
        }

        // Make sure the RNG state is still changing periodically. If not, we've hit a stop condition or error.
        private async Task<bool> CheckIfRNGStatePaused(ulong s0, ulong s1, CancellationToken token)
        {
            var (_s0, _s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
            if (GetAdvancesPassed(s0, s1, _s0, _s1) == 0)
            {
                Log("RNG state has stopped changing... ending DexFlip routine!");
                return true;
            }
            return false;
        }
    }
}
