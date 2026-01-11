using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon;

public sealed class EncounterBotPumpkabooLZA(PokeBotState Config, PokeTradeHub<PA9> Hub) : EncounterBotLZA(Config, Hub)
{
    int counter = 1;

    protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
    {
        Log("Starting Pumpkaboo routine...");

        while (!token.IsCancellationRequested)
        {
            Log($"Attempt #{counter}");
            Log("Checking if we're on the overworld...");
            while (!await IsOnOverworld(token).ConfigureAwait(false))
                await Click(B, 0_200, token).ConfigureAwait(false);
            await Task.Delay(1_200, token).ConfigureAwait(false);

            // If we're aggro'd, then we need to run out the door manually.
            if (await IsInBattle(token).ConfigureAwait(false))
            {
                Log("Escaping from aggressive Pokémon...");
                await SetStick(LEFT, 0, -30_000, 0_100, token).ConfigureAwait(false);
                await ResetStick(token).ConfigureAwait(false);
                await Click(LSTICK, 0_100, token).ConfigureAwait(false);

                // Mash Y to tumble 3 times.
                for (var i = 0; i < 20; i++)
                    await Click(Y, 0_100, token).ConfigureAwait(false);

                // Mash A to get out the door.
                for (var i = 0; i < 10; i++)
                    await Click(A, 0_100, token).ConfigureAwait(false);

                // Wait until we have control again.
                while (!await IsOnOverworld(token).ConfigureAwait(false))
                    await Task.Delay(0_100, token).ConfigureAwait(false);

                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, 11_000, -19_000, 0_100, token).ConfigureAwait(false);
                await ResetStick(token).ConfigureAwait(false);

                if (counter++ % 300 == 0) // Approx number that can be done at night, which is shorter than day.
                {
                    await ResetTimeOfDay(token).ConfigureAwait(false);
                    continue;
                }
            }
            else
            {
                await OpenMap(token).ConfigureAwait(false);
                await SetStick(LEFT, -25_000, 8_000, 0_070, token).ConfigureAwait(false);
                await ResetStick(token).ConfigureAwait(false);

                if (counter++ % 300 == 0) // Approx number that can be done at night, which is shorter than day.
                {
                    await ResetTimeOfDay(token).ConfigureAwait(false);
                    continue;
                }
            }

            for (var i = 0; i < 5; i++)
                await Click(A, 0_200, token).ConfigureAwait(false);
        }
    }

    private async Task OpenMap(CancellationToken token)
    {
        Log("Opening the map.");
        await Click(PLUS, 0_800, token).ConfigureAwait(false);
    }

    private async Task ResetTimeOfDay(CancellationToken token)
    {
        // We want to be hovered over Cafe Ultimo on the map at this point so the menu will show the correct options.
        Log("Leaving to reset the time of day.");
        await Click(Y, 0_300, token).ConfigureAwait(false);

        // Select and fly to Hibernal Pokémon Center
        await Click(DUP, 0_300, token).ConfigureAwait(false);
        for (var i = 0; i < 5; i++)
            await Click(A, 0_200, token).ConfigureAwait(false);
        while (!await IsOnOverworld(token).ConfigureAwait(false))
            await Click(B, 0_200, token).ConfigureAwait(false);
        await Task.Delay(1_200, token).ConfigureAwait(false);

        // Walk up to the bench
        await SetStick(LEFT, -32768, 12_000, 0_150, token).ConfigureAwait(false);
        for (var i = 0; i < 30; i++)
            await Click(A, 0_200, token).ConfigureAwait(false);
        await ResetStick(token).ConfigureAwait(false);
        while (!await IsOnOverworld(token).ConfigureAwait(false))
            await Click(B, 0_200, token).ConfigureAwait(false);

        // Click the bench again to flip the time back
        await SetStick(LEFT, 0, -32768, 0_300, token).ConfigureAwait(false);
        for (var i = 0; i < 30; i++)
            await Click(A, 0_200, token).ConfigureAwait(false);
        await ResetStick(token).ConfigureAwait(false);
        while (!await IsOnOverworld(token).ConfigureAwait(false))
            await Click(B, 0_200, token).ConfigureAwait(false);

        // Fly back to Cafe Ultimo
        await OpenMap(token).ConfigureAwait(false);
        await SetStick(LEFT, -5_000, -5_000, 0_200, token).ConfigureAwait(false);
        await Click(Y, 0_300, token).ConfigureAwait(false);
        await Click(DDOWN, 0_300, token).ConfigureAwait(false);
        for (var i = 0; i < 5; i++)
            await Click(A, 0_200, token).ConfigureAwait(false);
    }
}
