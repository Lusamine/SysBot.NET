using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotRoamerFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private uint OverworldOffset;
        private int ExtraTimeReset;
        private int ExtraTimeEncounter;
        private string TitleID = string.Empty;

        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            await InitializeSessionValues(sav, token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                if (Hub.Config.EncounterFRLG.LogInitialSeed)
                {
                    // Log the initial seed.
                    var seed = await GetInitialRNGState(false, token).ConfigureAwait(false);
                    Log($"Initial seed: {seed:x4}");
                }

                Log("Looking for a Pokémon...");
                var tries = 0;

                Roamer3? roamer;

                // Waits a random number of milliseconds to increase the number of possible RNG states.
                if (ExtraTimeEncounter > 0)
                {
                    var extra = Util.Rand.Next(0, ExtraTimeEncounter);
                    await Task.Delay(extra, token).ConfigureAwait(false);
                    Log($"Waiting an extra {extra}ms before encountering to increase RNG variability.");
                }

                for (var i = 0; i < 50; i++)
                    await Click(A, Util.Rand.Next(0_050, 0_200), token).ConfigureAwait(false);
                do
                {
                    roamer = await FindRoamer(token).ConfigureAwait(false);
                    if (++tries > 1000)
                        break;
                } while (roamer is null || roamer.Species == 0);

                // Check for a match
                if (roamer is not null && await HandleEncounter(roamer, token).ConfigureAwait(false))
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
            ExtraTimeEncounter = Hub.Config.EncounterFRLG.RandomTimeBeforeEncounter;
            ExtraTimeReset = Hub.Config.EncounterFRLG.RandomTimeSoftReset;
            TitleID = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);
        }

        private async Task<Roamer3?> FindRoamer(CancellationToken token)
        {
            while (!await IsInBattle(token).ConfigureAwait(false))
            {
                await Click(B, Util.Rand.Next(0_050, 0_200), token).ConfigureAwait(false);
                await Click(B, Util.Rand.Next(0_050, 0_200), token).ConfigureAwait(false);
                await Click(B, Util.Rand.Next(0_050, 0_200), token).ConfigureAwait(false);
                var roamer = await GetRoamerData(TitleID, token).ConfigureAwait(false);
                if (roamer.Species != 0)
                {
                    Log($"Encountered the roaming Pokémon {GetSpeciesName(roamer.Species)}!");
                    return roamer;
                }
            }
            return null;
        }
    }
}
