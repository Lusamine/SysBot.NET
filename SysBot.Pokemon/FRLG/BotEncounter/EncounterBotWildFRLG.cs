using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsFRLG;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotWildFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private uint EncounterOffset;
        private bool Horizontal; // Used for regular wild encounter slots only.
        private EncounterModeFRLG EncounterMode = EncounterModeFRLG.SlotsFRLG;

        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            await InitializeSessionValues(sav).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                Log("Looking for a Pokémon...");
                var tries = 0;

                PK3? pknew;

                do
                {
                    // Use the correct method to trigger a battle.
                    if (EncounterMode == EncounterModeFRLG.FishingFRLG)
                        await TryFish(token).ConfigureAwait(false);
                    else
                        await WiggleInPlace(token).ConfigureAwait(false);

                    pknew = await ReadUntilPresent(EncounterOffset, 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                    if (++tries > 1000)
                        break;
                } while (pknew is null || pknew.Species == 0);

                // Check for a match
                if (pknew is not null && await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                Log("No match, running away...");
                await EscapeBattle(token).ConfigureAwait(false);

                Horizontal = !Horizontal;
            }
        }

        // These don't change per session, and we access them frequently, so set these each time we start.
        private async Task InitializeSessionValues(SAV3FRLG sav)
        {
            Log("Initializing session constants...");

            EncounterMode = Hub.Config.EncounterFRLG.EncounteringType;
            Log($"Encountering type is set to {EncounterMode}.");
            if (EncounterMode != EncounterModeFRLG.SlotsFRLG && EncounterMode != EncounterModeFRLG.FishingFRLG)
                throw new System.Exception("Encountering type is not handled. Select from Slots or Fishing.");

            EncounterOffset = LanguageVersionOffsetsFRLG.GetWildPokemonOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
            Horizontal = true; // Always assume the user starts the bot facing up/down, and our first wiggles are left/right.
        }

        private async Task WiggleInPlace(CancellationToken token)
        {
            while (!await IsInBattle(token).ConfigureAwait(false))
            {
                if (Horizontal)
                {
                    await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
                    await Task.Delay(0_120, token).ConfigureAwait(false);
                    await Click(DLEFT, 0_050, token).ConfigureAwait(false);
                    await Task.Delay(0_120, token).ConfigureAwait(false);
                }
                else
                {
                    await Click(DUP, 0_050, token).ConfigureAwait(false);
                    await Task.Delay(0_120, token).ConfigureAwait(false);
                    await Click(DDOWN, 0_050, token).ConfigureAwait(false);
                    await Task.Delay(0_120, token).ConfigureAwait(false);
                }
            }
            await Task.Delay(1_000, token).ConfigureAwait(false); // Extra wait so the encounter is properly loaded.
        }

        private async Task TryFish(CancellationToken token)
        {
            while (!await IsInBattle(token).ConfigureAwait(false))
            {
                await Click(Y, 0_100, token).ConfigureAwait(false);
                await Click(A, 0_100, token).ConfigureAwait(false);
            }
            await Task.Delay(1_000, token).ConfigureAwait(false); // Extra wait so the encounter is properly loaded.
        }

        private async Task EscapeBattle(CancellationToken token)
        {
            while (!await IsOnBattleMenu(token).ConfigureAwait(false))
                await Click(B, 0_200, token).ConfigureAwait(false);

            Log("Initiating run routine.");
            await Click(DRIGHT, 0_200, token).ConfigureAwait(false);
            await Click(DDOWN, 0_200, token).ConfigureAwait(false);
            while (await IsInBattle(token).ConfigureAwait(false))
                await Click(A, 0_200, token).ConfigureAwait(false);
            await Task.Delay(0_800, token).ConfigureAwait(false);
        }
    }
}
