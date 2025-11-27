using PKHeX.Core;
using PKHeX.Core.Searching;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotResetLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : EncounterBotLGPE(cfg, hub)
    {
        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            var monoffset = GetResetOffset(Hub.Config.EncounterLGPE.EncounteringType);
            PB7 pkprev = new();
            PB7? pknew;

            while (!token.IsCancellationRequested)
            {
                // Reset the nature each round.
                await RefreshEncounterSettings(Hub, false, false, true, token);

                // Keep pressing A until we find a new Pokémon.
                Log("Going through menus...");
                do
                {
                    await DoExtraCommands(Hub.Config.EncounterLGPE.EncounteringType, token).ConfigureAwait(false);
                    pknew = await ReadUntilPresent(monoffset, 0_050, 0_050, token).ConfigureAwait(false);
                } while (pknew == null || SearchUtil.HashByDetails(pkprev) == SearchUtil.HashByDetails(pknew));

                if (await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                pkprev = pknew;

                if (Hub.Config.EncounterLGPE.EncounteringType is EncounterModeLGPE.GOParkLGPE)
                {
                    // GO Park encounter takes a while to load, and if we check too soon, we're still on the overworld.
                    await Task.Delay(6_000, token).ConfigureAwait(false);
                    Log("No match, running away...");
                    if (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                        await FleeToOverworld(token).ConfigureAwait(false);
                    continue;
                }

                Log("No match, resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
            }
        }

        private async Task DoExtraCommands(EncounterModeLGPE mode, CancellationToken token)
        {
            switch (mode)
            {
                case EncounterModeLGPE.StaticLGPE:
                    for (int i = 0; i < 2; i++)
                        await Click(A, 0_200, token).ConfigureAwait(false);
                    await Click(PLUS, 0_200, token).ConfigureAwait(false);
                    break;
                default:
                    for (int i = 0; i < 2; i++)
                        await Click(A, 0_200, token).ConfigureAwait(false);
                    break;
            }
        }

        private static uint GetResetOffset(EncounterModeLGPE mode)
        {
            return mode switch
            {
                EncounterModeLGPE.GOParkLGPE => LGPEGoParkOffset,
                EncounterModeLGPE.StaticLGPE => LGPEStaticOffset,
                _ => LGPEWildOffset,
            };
        }
    }
}
