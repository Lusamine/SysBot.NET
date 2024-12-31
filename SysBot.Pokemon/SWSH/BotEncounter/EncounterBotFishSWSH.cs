using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotFishSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                PK8? pknew;

                Log("Fishing for a Pokémon...");
                while (await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                    await Click(A, 0_400, token).ConfigureAwait(false);

                await Task.Delay(1_700, token).ConfigureAwait(false);

                Log("Waiting for a bite...");
                var timer = 20_000;
                while (!await IsFishingReady(token).ConfigureAwait(false) && timer > 0)
                    timer -= 0_050;

                // We've failed somehow, so reset. Battle check in case we got ambushed.
                if (timer <= 0)
                {
                    Log("Missed the bite, trying again.");
                    if (await IsInBattle(token).ConfigureAwait(false))
                        await FleeToOverworld(token).ConfigureAwait(false);
                    continue;
                }

                Log("Reeling it in!");
                timer = 3_000;
                while (timer > 0 && !await IsInBattle(token).ConfigureAwait(false))
                {
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    timer -= 0_050;
                }

                pknew = await ReadUntilPresent(WildPokemonOffset, 0_200, 0_200, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pknew == null)
                {
                    Log("Fish got away, trying again.");
                    await FleeToOverworld(token).ConfigureAwait(false);
                    continue;
                }

                while (!await IsOnBattleMenu(token).ConfigureAwait(false))
                    await Task.Delay(0_100, token).ConfigureAwait(false);
                await Task.Delay(0_100, token).ConfigureAwait(false);

                if (await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                Log("Running away...");
                await FleeToOverworld(token).ConfigureAwait(false);

                // Extra delay to be sure we're fully out of the battle.
                await Task.Delay(1_000, token).ConfigureAwait(false);
            }
        }

        private async Task<bool> IsFishingReady(CancellationToken token)
        {
            var FishingOffset = Version == GameVersion.SH ? FishingOffsetSH : FishingOffsetSW;
            var data = await Connection.ReadBytesAsync(FishingOffset, 1, token).ConfigureAwait(false);
            return data[0] == 1;
        }
    }
}
