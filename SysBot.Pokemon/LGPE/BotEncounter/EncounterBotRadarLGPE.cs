using PKHeX.Core;
using PKHeX.Core.Searching;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotRadarLGPE : EncounterBotLGPE
    {
        public EncounterBotRadarLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : base(cfg, hub)
        {
        }

        int natureCount;

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time for faster sending of commands.
            await SetMainLoopSleepTime(35, token).ConfigureAwait(false);

            var pknew = new PB7(); // For detecting Pokémon that run into us.

            await RefreshEncounterSettings(Hub, true, true, true, token);

            await ClearLastSpawnedSpecies(token).ConfigureAwait(false);

            // Intended to stand and idle. You can edit this to walk or reset a map in between scanning.
            while (!token.IsCancellationRequested)
            {
                // If we're in a battle, check it and run away if not a match.
                if (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                {
                    // Track the last encounter for comparisons since it remains until a new encounter.
                    var pkoriginal = pknew;

                    do
                    {
                        pknew = await ReadUntilPresent(LGPEWildOffset, 0_050, 0_050, token).ConfigureAwait(false);
                    } while (pknew == null || SearchUtil.HashByDetails(pkoriginal) == SearchUtil.HashByDetails(pknew));

                    if (await HandleEncounter(pknew, token).ConfigureAwait(false))
                        return;

                    Log("Running away...");
                    while (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                        await FleeToOverworld(token).ConfigureAwait(false);
                }

                // Show all radar results.
                (bool match, int radarpk) = await CheckRadarEncounter(token).ConfigureAwait(false);
                if (match)
                {
                    // Renew the fortuneteller on matching, then exit out.
                    // The other settings won't work after it's already spawned.
                    await RefreshEncounterSettings(Hub, false, false, true, token);
                    return;
                }

                if (radarpk < 0)
                    continue;

                // Periodically resets the fortuneteller since it'll wear out if a day passed.
                if (++natureCount % 100 == 0)
                    await RefreshEncounterSettings(Hub, false, false, true, token);
            }
        }
    }
}
