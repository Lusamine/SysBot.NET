using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsLZA;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotResetLZA(PokeBotState cfg, PokeTradeHub<PA9> hub) : EncounterBotLZA(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private ulong BoxStartOffset;

        protected override async Task EncounterLoop(SAV9ZA sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                await InitializeSessionOffsets(token).ConfigureAwait(false);

                Log("Looking for a Pokémon...");
                var tries = 0;

                PA9? pknew;
                var offset = BoxStartOffset;
                do
                {
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    pknew = await ReadUntilPresent(offset, 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                    if (++tries > 1000)
                        break;
                } while (pknew is null || pknew.Species == 0);

                // Check for a match
                if (pknew is not null && await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                Log("No match, resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
            }
        }

        // These don't change per session, and we access them frequently, so set these each time we start.
        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            BoxStartOffset = await SwitchConnection.PointerAll(Offsets.BoxStartPokemonPointer, token).ConfigureAwait(false);
        }
    }
}
