using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsFRLG;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotResetFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private uint EncounterOffset;
        private int PartyCount;
        private uint OverworldOffset;
        private int ExtraTimeReset;
        private int ExtraTimeEncounter;

        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            await InitializeSessionValues(sav, token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                Log("Looking for a Pokémon...");
                var tries = 0;

                PK3? pknew;

                // Waits a random number of milliseconds to increase the number of possible RNG states.
                if (ExtraTimeEncounter > 0)
                {
                    var extra = Util.Rand.Next(0, ExtraTimeEncounter);
                    await Task.Delay(extra, token).ConfigureAwait(false);
                    Log($"Waiting an extra {extra}ms before encountering to increase RNG variability.");
                }

                do
                {
                    await Click(A, 0_500, token).ConfigureAwait(false);
                    pknew = await ReadUntilPresent(EncounterOffset, 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                    if (++tries > 1000)
                        break;
                } while (pknew is null || pknew.Species == 0);

                // Check for a match
                if (pknew is not null && await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                Log("No match, resetting the game...");
                await SoftResetGame(OverworldOffset, ExtraTimeReset, token).ConfigureAwait(false);
            }
        }

        // These don't change per session, and we access them frequently, so set these each time we start.
        private async Task InitializeSessionValues(SAV3FRLG sav, CancellationToken token)
        {
            Log("Initializing session constants...");
            OverworldOffset = LanguageVersionOffsetsFRLG.GetOverworldOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);

            switch (Hub.Config.EncounterFRLG.EncounteringType)
            {
                case EncounterModeFRLG.StaticFRLG:
                    EncounterOffset = LanguageVersionOffsetsFRLG.GetWildPokemonOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
                    break;
                case EncounterModeFRLG.StarterFRLG:
                    PartyCount = 0;
                    EncounterOffset = LanguageVersionOffsetsFRLG.GetPartyStartOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
                    break;
                case EncounterModeFRLG.GiftFRLG:
                    PartyCount = await GetPartyCount(token).ConfigureAwait(false);
                    if (PartyCount == 6)
                        throw new System.Exception("All 6 party slots are full. Clear out at least one empty slot for gift encounters.");

                    EncounterOffset = LanguageVersionOffsetsFRLG.GetPartyStartOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
                    EncounterOffset += (uint)(PartyCount * 0x64); // The next empty slot.
                    break;
                default:
                    throw new System.Exception("Encountering type is not handled. Select from Static, Starter, or Gift.");
            }

            ExtraTimeEncounter = Hub.Config.EncounterFRLG.RandomTimeBeforeEncounter;
            ExtraTimeReset = Hub.Config.EncounterFRLG.RandomTimeSoftReset;
        }
    }
}
