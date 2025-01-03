using PKHeX.Core;
using SysBot.Base;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCurryRNGSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        int CurriesMade = 0;
        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time can allow for faster sending of commands.
            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.mainLoopSleepTime, 10, UseCRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            await EnterCampCurry(token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                await SetUpCurry(token).ConfigureAwait(false);

                ulong s0 = 0, s1 = 0;

                var start_timer = 100_000;
                var safety_timer = 8_000;
                while (!token.IsCancellationRequested)
                {
                    // Very rarely, the RNG state stops advancing. This is a safety to reboot for that.
                    if (--start_timer == 0)
                    {
                        Log("Unable to find an appropriate RNG state to start the curry. Resetting the game!");
                        await ResetGameCurry(token).ConfigureAwait(false);
                        start_timer = 100_000;
                        continue;
                    }

                    // Monitor for the perfect RNG state to start.
                    var (_s0, _s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
                    // Skip checking if it hasn't changed.
                    if (s0 == _s0 && s1 == _s1)
                        continue;

                    var output = GetSeedMonitorOutput(_s0, _s1, Hub.Config.EncounterSWSH.DisplaySeedMode);
                    Log($"Checking RNG state: {output}");

                    var (found, slotroll) = CheckForCurrySpawn(_s0, _s1);

                    // Save our RNG states to check next time.
                    (s0, s1) = (_s0, _s1);

                    if (found)
                    {
                        await Task.Delay(0_300, token).ConfigureAwait(false);
                        // Check if it's changed. Helps avoid really fast runs of advancements.
                        (_s0, _s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);

                        if (s0 != _s0 || s1 != _s1)
                        {
                            Log("Advancements are going too fast, holding out for a safer frame!");
                            await Task.Delay(safety_timer, token).ConfigureAwait(false);
                            safety_timer += 3_000; // Wait longer if we keep triggering this.
                            if (safety_timer > 60_000)
                            {
                                Log("Unable to find an appropriate RNG state to start the curry. Resetting the game!");
                                await ResetGameCurry(token).ConfigureAwait(false);
                                start_timer = 100_000;
                            }
                            (s0, s1) = (_s0, _s1);
                            continue;
                        }
                        Log($"Found a curry spawn, slot roll {slotroll}!");
                        break;
                    }
                }

                Log("Starting the curry...");
                for (int i = 0; i < 20; i++)
                    await Click(A, 0_100, token).ConfigureAwait(false);
                await Click(A, 36_000, token).ConfigureAwait(false);

                // Check how many advances went by.
                var (_s0_check, _s1_check) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
                var passed = GetAdvancesPassed(s0, s1, _s0_check, _s1_check);
                Log($"Advances passed: {passed}");
                if (passed < CurryAdvances[2])
                {
                    Log("Possible desync detected. Resetting the game.");
                    await ResetGameCurry(token).ConfigureAwait(false);
                    continue;
                }

                // Start pressing A because putting your heart in it ASAP saves a few seconds.
                for (var i = 0; i < 20; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);

                await Task.Delay(7_000, token).ConfigureAwait(false);
                Log("Tasting the curry...");
                await Click(A, 11_500, token).ConfigureAwait(false);

                Log("Returning to camp...");
                await Click(A, 4_000, token).ConfigureAwait(false);

                Log("Checking for a spawn.");
                var pk = await ReadUntilPresentPointer(CurrySpawnPointer, 3_000, 0_400, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pk == null)
                    Log("Invalid data detected. Restarting loop.");
                else if (await HandleEncounter(pk, token).ConfigureAwait(false))
                    return;

                if (++CurriesMade % Hub.Config.EncounterSWSH.Curry.CurryTimesToCook == 0)
                {
                    Log("Resetting the game to restore ingredients...");
                    await ResetGameCurry(token).ConfigureAwait(false);
                    continue;
                }

                await Click(X, 0_300, token).ConfigureAwait(false);
                // This should leave us on top of the Cooking button for next round.
            }
        }

        private async Task ResetGameCurry(CancellationToken token)
        {
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
            await Task.Delay(3_000, token).ConfigureAwait(false);
            await EnterCampCurry(token).ConfigureAwait(false);
        }

        // Enter camp.  Assumes your cursor is already over the camp button.
        private async Task EnterCampCurry(CancellationToken token)
        {
            Connection.Log("Setting up camp.");
            await Click(X, 1_000, token).ConfigureAwait(false);
            // Wait a generous amount of time because there's a good chance the Pokémon have high friendship.
            await Click(A, 12_000, token).ConfigureAwait(false);

            await Click(X, 0_300, token).ConfigureAwait(false);
            await Click(DRIGHT, 0_300, token).ConfigureAwait(false);
            // This should leave us on top of the Cooking button.
        }

        // Adds the ingredients for the curry and prepares to start.
        private async Task SetUpCurry(CancellationToken token)
        {
            Log("Starting a new curry...");
            // Enter the bag.
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 3_000, token).ConfigureAwait(false);

            // Dismiss the instruction menu.
            await Click(A, 0_700, token).ConfigureAwait(false);

            // Add the first ingredient.
            Log("Adding an ingredient.");
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 0_700, token).ConfigureAwait(false);

            // Dismiss the instruction menu.
            await Click(A, 0_700, token).ConfigureAwait(false);

            Log("Adding Berries.");
            await Click(A, 0_300, token).ConfigureAwait(false);
            var berry_count = Hub.Config.EncounterSWSH.Curry.CurryBerriesToUse;
            var clicks = berry_count - 1;
            if (berry_count > 6)
                clicks = 11 - berry_count;
            for (var i = 0; i < clicks; i++)
            {
                if (berry_count < 6)
                    await Click(DUP, 0_300, token).ConfigureAwait(false);
                else
                    await Click(DDOWN, 0_300, token).ConfigureAwait(false);
            }
            await Click(A, 2_000, token).ConfigureAwait(false);

            if (Hub.Config.EncounterSWSH.Curry.CurryBerriesToUse != 10)
                await Click(PLUS, 0_500, token).ConfigureAwait(false);
            await Task.Delay(1_000, token).ConfigureAwait(false);
            // End on the "Would you like to start cooking with your current Berry selection?" menu.
        }

        private (bool found, int slotroll) CheckForCurrySpawn(ulong s0, ulong s1)
        {
            var rng = new Xoroshiro128Plus(s0, s1);

            // Hardcode for party of 2. 1 is too slow to use, more than that are too fast or cause the pointer to change.
            // If you want to use a different party size, update the rest of the routine yourself.
            var advances = CurryAdvances[2];

            var spawn_chance = Hub.Config.EncounterSWSH.Curry.CurryTargetChance;
            var slot_total = (ulong)Hub.Config.EncounterSWSH.Curry.CurrySlotTotal;

            for (var i = 0; i < advances; i++)
                rng.Next();

            // The next one decides the spawn.
            var spawn_rand = rng.NextFloat(1);

            // Slot immediately after.
            var slot_rand = (int)rng.NextInt(slot_total);

            Log($"spawn_rand = {spawn_rand}, slot_rand = {slot_rand}");

            // Check for slot rand within range.
            if (spawn_rand < spawn_chance && EncounterSettings.IsSlotMatch(TargetSlots, slot_rand))
                return (true, slot_rand);

            return (false, slot_rand);
        }
    }
}
