using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon;

public sealed class EncounterBotSimpleTricksLZA(PokeBotState Config, PokeTradeHub<PA9> Hub) : EncounterBotLZA(Config, Hub)
{
    protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
    {
        Log("Starting simple automation routine...");

        // Perform the action indefinitely based on the EncounteringType.
        while (!token.IsCancellationRequested)
        {
            Log("Checking if we're on the overworld...");
            while (!await IsOnOverworld(token).ConfigureAwait(false))
                await Click(B, 0_200, token).ConfigureAwait(false);
            await Task.Delay(1_200, token).ConfigureAwait(false);

            await PerformMacro(Hub.Config.EncounterLZA.EncounteringType, token).ConfigureAwait(false);

            await ResetStick(token).ConfigureAwait(false);
            for (var i = 0; i < 8; i++)
                await Click(A, 0_200, token).ConfigureAwait(false);
        }
    }

    private async Task PerformMacro(EncounterModeLZA mode, CancellationToken token)
    {
        // Expect them to be the most zoomed out on the map.
        switch (mode)
        {
            case EncounterModeLZA.WildZone2LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, -1_000, 10_000, 0_300, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone3LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 0, 25_000, 0_100, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone5LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 0_300, 10_000, 0_300, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone8LZA:
                await Click(Y, 1_000, token).ConfigureAwait(false);
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, -10_000, -7_000, 0_300, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone10LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 10_000, 1_000, 0_400, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone14LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, -5_000, -10_000, 0_400, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone16LZA:
                await Click(Y, 1_000, token).ConfigureAwait(false);
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 10_000, 10_000, 0_300, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.WildZone19LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, -8_000, -10_000, 0_300, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.SewersRainLZA:
                await SetStick(LEFT, 0, -32768, 1_000, token).ConfigureAwait(false);
                break;

            case EncounterModeLZA.BattleZone17LZA:
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 10_000, 13_000, 0_400, token).ConfigureAwait(false);
                break;

                // Feel free to submit more simple tricks here!
        }
    }

    private async Task OpenMap(CancellationToken token)
    {
        Log("Opening the map.");
        await Click(PLUS, 0_800, token).ConfigureAwait(false);
    }
}
