using System;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.LegendEncounterInfo;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotOWLCheckRNGLA : EncounterBotLA
    {
        private readonly EncounterLASettings Settings;

        public EncounterBotOWLCheckRNGLA(PokeBotState cfg, PokeTradeHub<PA8> hub) : base(cfg, hub)
        {
            Settings = Hub.Config.EncounterLA;
        }

        // Cached offsets that stay the same per session.
        private ulong OverworldOffset;
        private ulong MapOffset;
        private ulong SpawnersOffset;

        // Tracks how many times we've tried.
        private int SpawnCounter;

        // Everything this scans for has 3 flawless IVs.
        private readonly int FlawlessIVs = 3;

        // Tracks whether this is the first time we are going to an area after a reset.
        bool first_time = true;

        protected override async Task EncounterLoop(SAV8LA sav, CancellationToken token)
        {
            await InitializeSessionOffsets(token).ConfigureAwait(false);
            var species = Settings.OWLegendary;
            var mode = GetLegendaryMode(species);
            var area = GetLegendaryArea(species);
            var spawner = GetLegendarySpawnerHash(species);

            while (!token.IsCancellationRequested)
            {
                if (mode is LegendResetMode.Wandering)
                {
                    if (first_time)
                    {
                        // Start in Jubilife in front of the guard.
                        await Click(A, 0_500, token).ConfigureAwait(false);
                        await Click(A, 1_000, token).ConfigureAwait(false);

                        await AdjustMap(area, token).ConfigureAwait(false);
                        first_time = false;
                    }

                    // Returns true if we found a matching seed.
                    if (await CycleWanderingLegends(species, area, spawner, token).ConfigureAwait(false))
                        return;
                }
                else if (mode is LegendResetMode.Cave)
                {
                    // Returns true if we found a matching seed.
                    if (await CycleCaveLegends(species, spawner, token).ConfigureAwait(false))
                        return;
                }
                else
                {
                    Log("Invalid mode.");
                    return;
                }
            }
        }

        // These don't change per session and we access them frequently, so set these each time we start.
        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            OverworldOffset = await SwitchConnection.PointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
            MapOffset = await SwitchConnection.PointerAll(Offsets.MapLocationPointerOffline, token).ConfigureAwait(false);
            SpawnersOffset = await SwitchConnection.PointerAll(Offsets.SpawnersPointer, token).ConfigureAwait(false);
        }

        private async Task AdjustMap(AreaID area, CancellationToken token)
        {
            Log("Adjusting the map...");
            while (!await IsHoveringMap((uint)area, token).ConfigureAwait(false))
                await Click(DRIGHT, 0_500, token).ConfigureAwait(false);
        }

        private async Task<bool> IsHoveringMap(uint mapValue, CancellationToken token)
        {
            byte[]? data = await SwitchConnection.ReadBytesAbsoluteAsync(MapOffset, 1, token).ConfigureAwait(false);
            return data[0] == mapValue;
        }

        private async Task<bool> CycleWanderingLegends(OWLegendary species, AreaID area, ulong spawner, CancellationToken token)
        {
            // Click once to engage the guard.
            await Click(A, 0_500, token).ConfigureAwait(false);
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
                await Click(A, 0_100, token).ConfigureAwait(false);

            // Extra wait in case of loading screen.
            await Task.Delay(1_000, token).ConfigureAwait(false);

            // Check the spawners.
            if (await CheckLegendarySeed(species, spawner, token).ConfigureAwait(false))
                return true;

            // Walk over to Laventon.
            await LeaveArea(area, token).ConfigureAwait(false);

            Log("Talking to Laventon...");
            await Click(A, 0_500, token).ConfigureAwait(false);
            await Click(A, 0_700, token).ConfigureAwait(false);
            await Click(A, 0_600, token).ConfigureAwait(false);
            await Click(DDOWN, 0_050, token).ConfigureAwait(false);

            // Click A until we get back to Jubilife.
            Log("Traveling back to Jubilife Village...");
            var timer = 0;
            while (!await IsOnOverworld(OverworldOffset, token).ConfigureAwait(false))
            {
                await Click(A, 0_100, token).ConfigureAwait(false);

                // Occasionally we miss Laventon while trying to get to Jubilife, so reset the game.
                if (++timer >= 200)
                {
                    await ReOpenGame(Hub.Config, token).ConfigureAwait(false);
                    await InitializeSessionOffsets(token).ConfigureAwait(false);
                    first_time = true;
                    return false;
                }
            }

            // Walk backwards to the exit again.
            Log("Leaving Jubilife Village...");
            await SetStick(LEFT, 0, -32768, 0_200, token).ConfigureAwait(false);
            await Click(LSTICK, 0, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -32768, 1_200, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            return false;
        }

        private async Task LeaveArea(AreaID areaId, CancellationToken token)
        {
            switch (areaId)
            {
                case AreaID.Fieldlands:
                    await SetStick(LEFT, 32_767, 32_767, 1_000, token).ConfigureAwait(false);
                    break;
                case AreaID.Mirelands:
                    await SetStick(LEFT, 32_767, 15_000, 1_200, token).ConfigureAwait(false);
                    break;
                case AreaID.Coastlands:
                    await SetStick(LEFT, 32_767, 10_000, 1_100, token).ConfigureAwait(false);
                    break;
                case AreaID.Highlands:
                    await SetStick(LEFT, 32_767, 21_500, 1_300, token).ConfigureAwait(false);
                    break;
                case AreaID.Icelands:
                    await SetStick(LEFT, 32_767, 17_000, 1_000, token).ConfigureAwait(false);
                    break;
            }
            await ResetStick(token).ConfigureAwait(false);
        }

        private async Task<bool> CycleCaveLegends(OWLegendary species, ulong spawner, CancellationToken token)
        {
            // Click A to enter the subarea.
            Log("Entering the cave...");
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Check the spawners.
            if (await CheckLegendarySeed(species, spawner, token).ConfigureAwait(false))
            {
                // Minimize the game if we find a match because we might be with some aggressive Pokémon.
                await Click(HOME, 1_600, token).ConfigureAwait(false);
                return true;
            }

            Log("No match, resetting the game...");
            await CloseGame(Hub.Config, token).ConfigureAwait(false);
            await StartGame(Hub.Config, token).ConfigureAwait(false);
            await InitializeSessionOffsets(token).ConfigureAwait(false);
            return false;
        }

        private async Task<bool> CheckLegendarySeed(OWLegendary species, ulong spawner, CancellationToken token)
        {
            // Count backwards because our legendary is closer to the end.
            for (ulong i = 400; i >= 0; i--)
            {
                byte[]? data = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnersOffset + (i * 0x440) + 0x410, 8, token).ConfigureAwait(false);
                ulong spawnerhash = BitConverter.ToUInt64(data, 0);
                if (spawnerhash != spawner)
                    continue;

                data = await SwitchConnection.ReadBytesAbsoluteAsync(SpawnersOffset + (i * 0x440) + 0x408, 8, token).ConfigureAwait(false);
                ulong seed = BitConverter.ToUInt64(data, 0);
                Log($"Checking spawner #{i}, hash 0x{spawnerhash:x16}, seed 0x{seed:x16}. Attempt #{++SpawnCounter}");
                Settings.AddCompletedLegends();

                if (IsMatch((int)species, seed))
                    return true;
                break;
            }
            return false;
        }

        // Generates the Pokémon so we can see if it matches the specifications.
        private bool IsMatch(int species, ulong seed)
        {
            ulong init = unchecked(seed);  // Generator seed
            Xoroshiro128Plus rng = new(init);

            for (var i = 0; i < Settings.SearchDepth; i++)
            {
                // Group seed generates the generator seed and alpha move seed.
                var genseed = rng.Next();
                var alphamoveseed = rng.Next();

                // Generator seed generates the slot, mon seed, and level.
                var slotrng = new Xoroshiro128Plus(genseed);
                _ = slotrng.Next(); // Slot, not used in this case.
                var mon_seed = slotrng.Next();
                // Level is ignored since everything has fixed level.

                if (GenerateAndCheck(species, mon_seed, i))
                    return true;

                // Reseed the RNG for the next genie.
                var newseed2 = rng.Next();
                rng = new Xoroshiro128Plus(newseed2);
            }

            return false;
        }

        private bool GenerateAndCheck(int species, ulong mon_seed, int advances)
        {
            Xoroshiro128Plus rng = new(mon_seed);

            // Encryption Constant
            uint ec = (uint)rng.NextInt();
            // Fake TID
            rng.NextInt();
            // PID - these are all never shiny
            uint pid = (uint)rng.NextInt();

            string ivstring = string.Empty;
            Span<int> ivs = [-1, -1, -1, -1, -1, -1];
            const int MAX = 31;
            for (int i = 0; i < FlawlessIVs; i++)
            {
                int index;
                do { index = (int)rng.NextInt(6); }
                while (ivs[index] != -1);

                ivs[index] = MAX;
            }

            for (int i = 0; i < ivs.Length; i++)
            {
                if (ivs[i] == -1)
                    ivs[i] = (int)rng.NextInt(32);
                ivstring += ivs[i];
                if (i < 5)
                    ivstring += "/";
            }

            if (StopConditionSettings.MatchesTargetIVs(ivs, DesiredMinIVs, DesiredMaxIVs))
                return false;

            // Ability
            rng.NextInt(2);

            // Gender
            var genderratio = PersonalTable.LA[species].Gender;
            var gender = "";
            if (genderratio is not (PersonalInfo.RatioMagicGenderless or PersonalInfo.RatioMagicFemale or PersonalInfo.RatioMagicMale))
            {
                var gender_val = (int)rng.NextInt(252) + 1 < genderratio ? 1 : 0;
                gender = gender_val == 0 ? " (M)" : " (F)";
            }

            int nature = (int)rng.NextInt(25);
            var target_nature = Hub.Config.StopConditions.TargetNature;
            if (target_nature != Nature.Random && nature != (int)target_nature)
                return false;

            // If we get to here, then everything matches.
            Log("Result found!");
            Log($"{GameInfo.GetStrings(1).Species[species]}{gender} in {advances} advance(s)!");
            Log($"IVs: {ivstring}, Nature: {GameInfo.GetStrings(1).Natures[nature]}");
            Log($"PID: 0x{pid:x8}, EC: 0x{ec:x8}");
            return true;
        }

        public enum AreaID
        {
            None = 0,
            Fieldlands = 7,
            Mirelands = 8,
            Coastlands = 9,
            Highlands = 10,
            Icelands = 11,
        }
    }
}
