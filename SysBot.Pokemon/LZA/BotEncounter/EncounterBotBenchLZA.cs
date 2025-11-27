using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon;

public sealed class EncounterBotBenchLZA(PokeBotState Config, PokeTradeHub<PA9> Hub) : EncounterBotLZA(Config, Hub)
{
    protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
    {
        Log("Starting bench routine...");

        while (!token.IsCancellationRequested)
        {
            await SetStick(LEFT, 0, -32768, 0_300, token).ConfigureAwait(false);
            await Click(A, 0_100, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            while (!await IsOnOverworld(token).ConfigureAwait(false))
                await Click(A, 0_200, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
        }
    }
}
