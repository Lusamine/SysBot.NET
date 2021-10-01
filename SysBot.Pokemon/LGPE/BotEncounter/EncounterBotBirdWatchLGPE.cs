using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotBirdWatchLGPE : EncounterBotLGPE
    {
        public EncounterBotBirdWatchLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : base(cfg, hub)
        {
        }

        private readonly int threshold = 4;
        private ulong BirdRNGOffset;

        bool IsLegendaryBird(int species) => species is (144 or 145 or 146);

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            // Reducing sys-botbase's sleep time for faster sending of commands.
            await SetMainLoopSleepTime(35, token).ConfigureAwait(false);

            bool match;
            int species_match;

            await UpdateBirdRNGPointer(token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                // Ensure we're on the overworld each pass.
                if (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                    await HandleAccidentalEncounter(token).ConfigureAwait(false);

                // Check our current RNG state for a bird within the provided range of advances.
                var (s0, s1) = await GetBirdRNGState(BirdRNGOffset, false, token).ConfigureAwait(false);

                var output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterLGPE.DisplaySeedMode);
                Log($"Checking RNG state: {output}");

                var (found, advances) = CheckForBirdWithinRange(s0, s1);
                if (!found)
                {
                    // Walk in and out of the Pokémon Center to get a new RNG state.
                    Log($"No match within {advances} advances, fetching a new RNG state...");
                    await EnterPokemonCenter(token).ConfigureAwait(false);
                    await LeavePokemonCenter(token).ConfigureAwait(false);
                    continue;
                }

                // Now we wait it out.
                Log("Waiting for the legendary bird to spawn.");
                if (advances >= threshold)
                    await RefreshEncounterSettings(Hub, false, Hub.Config.EncounterLGPE.SetMaxLureAdvancements, false, token);
                bool encounter_settings_set = false;

                while (!token.IsCancellationRequested)
                {
                    if (advances < threshold)
                    {
                        // Only set them once, should be close enough for them to be in effect during the spawn.
                        if (!encounter_settings_set)
                        {
                            output = GetSeedMonitorOutput(s0, s1, Hub.Config.EncounterLGPE.DisplaySeedMode);
                            Log($"Currently on RNG state: {output}");
                            encounter_settings_set = true;
                            Log("Waiting out the last few advancements...");
                            await RefreshEncounterSettings(Hub, true, Hub.Config.EncounterLGPE.SetMaxLureEncounter, true, token);
                        }

                        // Use radar to check if it matches stop conditions.
                        (match, species_match) = await CheckRadarEncounter(token).ConfigureAwait(false);
                        if (match && IsLegendaryBird(species_match))
                        {
                            Log("Legendary bird found!");
                            await MinimizeGame(token).ConfigureAwait(false);
                            return;
                        }

                        // Didn't match stop conditions. Once a bird spawns, we can exit out.
                        // Allowance in advances because it depends on whether it was spawned with or without lure.
                        if (advances <= 1 && IsLegendaryBird(species_match))
                        {
                            // They can choose whether to pause so they can manually catch every bird.
                            // If they set a species, only stop on that bird; otherwise stop on all birds.
                            var stop_species = (int)Hub.Config.StopConditions.StopOnSpecies;
                            var did_match_species = stop_species == (int)Species.None || stop_species == species_match;
                            if (Hub.Config.EncounterLGPE.PauseOnBirdMatch && did_match_species)
                            {
                                Log("Target bird spawned. Stopping bot routine.");
                                await MinimizeGame(token).ConfigureAwait(false);
                                return;
                            }
                            break;
                        }

                        // If we go this far, something has gone wrong.
                        if (advances < -10)
                        {
                            Log("Target bird did not spawn when expected. Ensure that you have caught the static legendary birds.");
                            break;
                        }
                    }

                    // Some of the ground encounters can wander into us, resetting the RNG state.
                    if (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                        break;

                    // Check how many advances went by.
                    var (_s0, _s1) = await GetBirdRNGState(BirdRNGOffset, false, token).ConfigureAwait(false);
                    var passed = GetAdvancesPassed(s0, s1, _s0, _s1);
                    if (passed == -1) // Something bad has happened.
                    {
                        // Wait a little longer if we trip this check instead of the previous one,
                        // because we may not have gotten to the point that we're not on the overworld.
                        await Task.Delay(3_000, token).ConfigureAwait(false);
                        break;
                    }
                    advances -= passed;

                    // Store the state for the next pass.
                    s0 = _s0;
                    s1 = _s1;
                }
                Log("Result not found, continuing the search.");
            }
        }

        private (bool found, int advances) CheckForBirdWithinRange(ulong s0, ulong s1)
        {
            var rng = new Xoroshiro128Plus(s0, s1);
            var maxAdvances = Hub.Config.EncounterLGPE.MaxBirdWatchAdvances;
            var targetBird = Hub.Config.StopConditions.StopOnSpecies;

            for (var advances = 0; advances < maxAdvances; advances++)
            {
                var check = rng.Next();
                var rand = (uint)(check % 10000);

                if (rand <= 4)
                {
                    var bird = (uint)(rng.Next() % 3);

                    if (!MatchTargetBird(targetBird, bird))
                        continue;

                    Log($"{GetBirdName(bird)} will be generated in {advances} advances!");
                    return (true, advances);
                }
            }
            return (false, maxAdvances);
        }

        private string GetBirdName(uint index)
        {
            return index switch
            {
                0 => "Moltres",
                1 => "Zapdos",
                2 => "Articuno",
                _ => "Unknown",
            };
        }

        // Used to check if the species in stop conditions matches the bird index provided.
        private bool MatchTargetBird(Species target, uint index)
        {
            if (target == Species.None || Hub.Config.StopConditions.StopOnAllBirdsLGPE)
                return true;
            if (target == Species.Articuno && index == 2)
                return true;
            if (target == Species.Zapdos && index == 1)
                return true;
            if (target == Species.Moltres && index == 0)
                return true;
            return false;
        }

        private async Task EnterPokemonCenter(CancellationToken token)
        {
            await SetStick(RIGHT, 0, 32767, 0_500, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            await UpdateBirdRNGPointer(token).ConfigureAwait(false);
            await Task.Delay(Hub.Config.EncounterLGPE.ExtraTimeEnterPMC, token).ConfigureAwait(false);
        }

        private async Task LeavePokemonCenter(CancellationToken token)
        {
            await SetStick(RIGHT, 0, -32767, 1_500, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            await UpdateBirdRNGPointer(token).ConfigureAwait(false);
            await Task.Delay(Hub.Config.EncounterLGPE.ExtraTimeExitPMC, token).ConfigureAwait(false);
        }

        private async Task HandleAccidentalEncounter(CancellationToken token)
        {
            Log("Found an accidental encounter. Restarting the search.");
            while (!await IsOnOverworldBattle(token).ConfigureAwait(false))
                await FleeToOverworld(token).ConfigureAwait(false);

            // Wait long enough for another spawn to alter the RNG state.
            await ClearLastSpawnedSpecies(token).ConfigureAwait(false);

            int radar_species = 0;
            while (radar_species == 0)
                (radar_species, _) = await GetLastSpawnedSpecies(token).ConfigureAwait(false);
        }

        private async Task UpdateBirdRNGPointer(CancellationToken token)
        {
            await SetMainLoopSleepTime(50, token).ConfigureAwait(false);
            bool valid = false;
            while (!valid)
                (valid, BirdRNGOffset) = await ValidatePointerAll(LGPEBirdRNGPointer, token).ConfigureAwait(false);
            await SetMainLoopSleepTime(35, token).ConfigureAwait(false);
            await Task.Delay(1_700, token).ConfigureAwait(false);
        }

        public async Task<(ulong s0, ulong s1)> GetBirdRNGState(ulong offset, bool log, CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, 16, token).ConfigureAwait(false);
            var s0 = BitConverter.ToUInt64(data, 0);
            var s1 = BitConverter.ToUInt64(data, 8);
            if (log)
                Log($"RNG state: {s0:x16}, {s1:x16}");
            return (s0, s1);
        }
    }
}
