using PKHeX.Core;
using SysBot.Base;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotTreeRNGSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        readonly ushort[] DexRecs = new ushort[4];

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time can allow for faster sending of commands.
            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.mainLoopSleepTime, 10, UseCRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

            PK8? pk;

            while (!token.IsCancellationRequested)
            {
                // We have to update every time we reset the game in case the day ticked over.
                // While rolling days, if the Pokédex isn't viewed, new recommendations aren't generated.
                Log("Updating Dex Recs...");
                await UpdateDexRecs(token).ConfigureAwait(false);

                // Click into the tree and check the RNG state.
                await Click(A, 1_000, token).ConfigureAwait(false);
                await Click(B, 0_500, token).ConfigureAwait(false);
                var (_s0, _s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
                var output = GetSeedMonitorOutput(_s0, _s1, Hub.Config.EncounterSWSH.DisplaySeedMode);
                Log($"Checking RNG state: {output}");

                var found = FindNextRNGStateMatch(_s0, _s1);
                if (!found)
                {
                    Log("No match, continuing...");
                    await Click(B, 0_500, token).ConfigureAwait(false);
                    continue;
                }

                // This RNG state matches what we want, so shake the tree!
                Log("Looking for a Pokémon...");
                do
                {
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    pk = await ReadUntilPresent(WildPokemonOffset, 0_200, 0_200, BoxFormatSlotSize, token).ConfigureAwait(false);
                } while (pk is null || !await IsInBattle(token).ConfigureAwait(false));

                while (!await IsOnBattleMenu(token).ConfigureAwait(false))
                    await Task.Delay(0_100, token).ConfigureAwait(false);

                if (await HandleEncounter(pk, token).ConfigureAwait(false))
                    return;

                Log("No match, resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
            }
        }

        private bool FindNextRNGStateMatch(ulong s0, ulong s1)
        {
            var rng = new Xoroshiro128Plus(s0, s1);

            // Roll the current tree's health.
            int tree_health = 100 - (int)rng.NextInt(59) - 21;
            Log($"Tree Health = {tree_health}");

            while (tree_health > 0)
            {
                // Each shake deducts rand7 + 8 health.
                var hit = (int)rng.NextInt(7) + 8;
                tree_health -= hit;
                //Log($"Deducted {hit} health.");
                if (tree_health <= 0)
                    break;

                // If the tree still has health left, randomize 1-3 berries.
                var berries = (int)rng.NextInt(3) + 1;
                //Log($"Generated {berries} berries.");
                for (var i = 0; i < berries; i++)
                    rng.NextInt(100);
            }

            // Tree slot table decided by: 1) Lead ability activation, 2) Dex Rec activation, 3) regular slots.
            // Lead ability activation. Configured for an active Harvest lead.
            var abil_active_rand = rng.NextInt(100);
            bool ability_active = abil_active_rand >= 49;

            uint target_species = (uint)Hub.Config.StopConditions.StopOnSpecies;
            uint species_final = (uint)Species.None;

            if (ability_active)
            {
                Log("Harvest activates!");
                // Since we're doing this at the Insular Sea, the only Pokémon that can appear with Harvest is Applin.
                // Otherwise, we would have to do another random roll between number of eligible Pokémon.
                // Logic included below for anyone who wants to try another table.

                // Make a list of Pokemon in Insular Sea that can appear with Harvest, then randomly pick one.
                var harvest_slots = InsularSea.Where(e => e.CanHarvest).ToArray();
                var harvest_count = (ulong)harvest_slots.Length;
                if (harvest_count > 1)
                {
                    var harvest_rand = rng.NextInt(harvest_count);
                    species_final = harvest_slots[harvest_rand].Species;
                }
                else if (harvest_count > 0)
                {
                    species_final = harvest_slots[0].Species;
                }

                if (species_final != (int)Species.None && target_species != (int)Species.None && target_species != species_final)
                    return false;
            }

            // Species wasn't chosen by type pulling ability.
            if (species_final == (int)Species.None)
            {
                // Dex Rec attempts this even if it's empty.
                var dexrec_rand = rng.NextInt(100);
                bool dexrec_active = dexrec_rand > 50;
                // Only rolls for a slot if DexRec has entries.
                if (dexrec_active && DexRecs[0] != (int)Species.None)
                {
                    var dexrec_slot = (int)rng.NextInt(4);
                    var species_dexrec = DexRecs[dexrec_slot];
                    if (species_dexrec == (int)Species.None)
                    {
                        Log("Dex Recommendation activates but no Pokémon are being recommended.");
                    }
                    else
                    {
                        Log($"Dex Recommendation activates for slot {dexrec_slot}: {GameInfo.GetStrings("en").Species[species_dexrec]}.");
                        // Check if the chosen species from dexrec is any of the species in InsularSea table.
                        if (InsularSea.Any(e => e.Species == species_dexrec))
                        {
                            species_final = species_dexrec;
                            if (target_species != (int)Species.None && target_species != species_final)
                                return false;
                        }
                        else
                        {
                            Log("Dex Recommendation species is not in encounter table, checking normal slots.");
                        }
                    }
                }
            }

            // Regular slots if we still haven't decided on a Pokemon.
            if (species_final == (int)Species.None)
            {
                var slot_rand = (int)rng.NextInt(100);
                species_final = FindSlotInTable((uint)slot_rand, InsularSea);
                Log($"Regular slot rand: {slot_rand}, {GameInfo.GetStrings("en").Species[(int)species_final]}.");
                if (target_species != (int)Species.None && target_species != species_final)
                    return false;
            }

            // Level range for Insular Sea is 43-48.
            var level_rand = (int)rng.NextInt(6);
            //Log($"Level = {43 + level_rand}");

            // Mark
            var (found, mark) = CheckPredictedMark(ref rng, true);
            if (found)
                Log($"Mark = {mark}");
            else
                Log("No mark found.");

            return found && !UnwantedMarks.Contains(mark);
        }

        private static (bool found, string mark) CheckPredictedMark(ref Xoroshiro128Plus rng, bool mark_charm)
        {
            var rolls = mark_charm ? 3 : 1;
            for (var i = 0; i < rolls; i++)
            {
                var rare_mark = rng.NextInt(1000);
                var personality_mark = rng.NextInt(100);
                var uncommon_mark = rng.NextInt(50);
                var weather_mark = rng.NextInt(50);
                var time_mark = rng.NextInt(50);
                var fishing_mark = rng.NextInt(25);

                if (rare_mark == 0)
                    return (true, "Rare Mark");
                if (personality_mark == 0)
                {
                    var specific_personality = 70 + rng.NextInt(28);
                    var ribbon = (RibbonIndex)specific_personality;
                    return (true, RibbonStrings.GetName($"Ribbon{ribbon}"));
                }
                if (uncommon_mark == 0)
                    return (true, "Uncommon Mark");
                if (weather_mark == 0)
                    return (true, "Weather Mark");
                if (time_mark == 0)
                    return (true, "Time Mark");
                //if (fishing_mark == 0)
                //    return (true, "Fishing Mark");
            }
            return (false, "");
        }

        private async Task UpdateDexRecs(CancellationToken token)
        {
            for (uint i = 0; i < 4; i++)
            {
                // Each dex rec is 32 bytes long. The first 2 bytes are the species.
                var data = await SwitchConnection.ReadBytesAbsoluteAsync(DexRecOffset + (0x20 * i), 2, token).ConfigureAwait(false);
                DexRecs[i] = BitConverter.ToUInt16(data, 0);
            }
            var dexrecs = string.Join(", ", DexRecs.Select(i => GameInfo.GetStrings("en").Species[i]));
            Log($"Dex Recs: {dexrecs}");
        }

        public class EncounterTableEntry(uint species, uint min, uint max, bool can_harvest)
        {
            public uint Species = species;
            public uint Min = min;
            public uint Max = max;
            public bool CanHarvest = can_harvest;
        }

        public static uint FindSlotInTable(uint rand, EncounterTableEntry[] table)
        {
            foreach (EncounterTableEntry entry in table)
            {
                if (rand >= entry.Min && rand <= entry.Max)
                    return entry.Species;
            }
            return (int)Species.None;
        }

        public static readonly EncounterTableEntry[] InsularSea =
        [
            new((uint)Species.Skwovet,  0, 59, false),
            new((uint)Species.Applin,  60, 89, true),
            new((uint)Species.Skwovet, 90, 99, false),
        ];
    }
}
