using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon;

public sealed class EncounterBotASpamLZA(PokeBotState Config, PokeTradeHub<PA9> Hub) : EncounterBotLZA(Config, Hub)
{
    protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
    {
        Log("Mashing the A button...");

        while (!token.IsCancellationRequested)
            await Click(A, 0_100, token).ConfigureAwait(false);
    }
}
