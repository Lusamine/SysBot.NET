using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotVirizionLZA(PokeBotState cfg, PokeTradeHub<PA9> hub) : EncounterBotLZA(cfg, hub)
    {
        protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
        {
            Log("Walking in a loop...");
            var tries = Hub.Config.EncounterLZA.LegendLoops;

            while (!token.IsCancellationRequested)
            {
                await SetStick(LEFT, 0, 32_767, 0_100, token).ConfigureAwait(false);
                await Click(B, 0, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, 32_767, 3_300, token).ConfigureAwait(false);
                await SetStick(LEFT, 0_100, 0, 0_080, token).ConfigureAwait(false);

                await SetStick(LEFT, 0, -32_767, 0_100, token).ConfigureAwait(false);
                await Click(B, 0, token).ConfigureAwait(false);
                await SetStick(LEFT, 0, -32_767, 3_100, token).ConfigureAwait(false);
                await SetStick(LEFT, -0_100, 0, 0_080, token).ConfigureAwait(false);

                if (--tries <= 0)
                {
                    await Click(X, 1_000, token).ConfigureAwait(false);
                    Log("Routine complete and game paused!");
                    return;
                }
            }
        }
    }
}
