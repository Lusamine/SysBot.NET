using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotGiftLGPE : EncounterBotLGPE
    {
        public EncounterBotGiftLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : base(cfg, hub)
        {
        }

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                int radarpk;
                bool match;

                Log("Going through menus...");

                // Reset the nature each round.
                await RefreshEncounterSettings(Hub, false, false, true, token);

                // Allows us to see multiple Pokémon before resetting the game.
                for (int i = 0; ;)
                {
                    await ClearLastSpawnedSpecies(token).ConfigureAwait(false);

                    // Keep pressing A until we find a new Pokémon.
                    do
                    {
                        await Click(A, 0_200, token).ConfigureAwait(false);
                        (match, radarpk) = await CheckRadarEncounter(token).ConfigureAwait(false);
                    } while (radarpk == -1);

                    if (match)
                        return;

                    if (++i >= Hub.Config.EncounterLGPE.GiftTradePokemonCount)
                        break;

                    while (!await IsOnOverworldStandard(token).ConfigureAwait(false))
                        await Click(B, 0_200, token).ConfigureAwait(false);
                }

                Log("No match, resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
            }
        }
    }
}
