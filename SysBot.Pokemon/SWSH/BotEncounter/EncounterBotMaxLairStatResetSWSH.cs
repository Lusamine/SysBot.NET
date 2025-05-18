using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotMaxLairStatResetSWSH : EncounterBotSWSH
    {
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;

        private static readonly string[] StatNames = ["HP", "Atk", "Def", "SpA", "SpD", "Spe"];

        public EncounterBotMaxLairStatResetSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : base(cfg, hub)
        {
            StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        }

        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            if (Hub.Config.EncounterSWSH.MaxLairNoteToPick > 4 || Hub.Config.EncounterSWSH.MaxLairNoteToPick < 1)
            {
                Log("MaxLairNoteToPick is out of range, resetting to 1.");
                Hub.Config.EncounterSWSH.MaxLairNoteToPick = 1;
            }
            if (Hub.Config.EncounterSWSH.MaxLairRentalToPick > 3 || Hub.Config.EncounterSWSH.MaxLairRentalToPick < 1)
            {
                Log("MaxLairRentalToPick is out of range, resetting to 1.");
                Hub.Config.EncounterSWSH.MaxLairRentalToPick = 1;
            }

            while (!token.IsCancellationRequested)
            {
                // Reset the Max Lair penalty and clear the Scientist's warning.
                await ClearMaxLairPenalty(token).ConfigureAwait(false);

                // Should start in front of Scientist with no penalty.
                // Timings are optimized for an English game with Text speed of Fast. Adjust if this doen't work for you.
                Log("Starting a new Dynamax Adventure.");
                await Click(A, 0_700, token).ConfigureAwait(false);
                await Click(A, 0_400, token).ConfigureAwait(false);
                await Click(A, 1_300, token).ConfigureAwait(false);
                await Click(A, 0_700, token).ConfigureAwait(false);
                await Click(A, 0_400, token).ConfigureAwait(false);

                Log("Selecting a Pokémon from the notes...");
                var note_adjust = Hub.Config.EncounterSWSH.MaxLairNoteToPick - 1;
                for (var i = 0; i < note_adjust; i++)
                    await Click(DDOWN, 0_300, token).ConfigureAwait(false);

                Log("Accepting and saving...");
                for (var i = 0; i < 5; i++)
                    await Click(A, 1_000, token).ConfigureAwait(false);
                await Task.Delay(1_000, token).ConfigureAwait(false);

                // Lobby should load here. Click down to "Don't Invite Others".
                await Click(DDOWN, 0_300, token).ConfigureAwait(false);

                Log("Entering the rental lobby.");
                await Click(A, 1_000, token).ConfigureAwait(false);

                // We should be able to read the RNG state now.
                bool valid = false;
                ulong ofs = 0;
                while (!valid)
                    (valid, ofs) = await ValidatePointerAll(MaxLairPokemonRNGPointer, token).ConfigureAwait(false);
                var (s0, s1) = await GetMaxLairRNGState(ofs, true, token).ConfigureAwait(false);

                var (match, msg) = IsMatchMaxLairLegendary(s0, s1);
                if (match)
                {
                    // Wait a little to ensure the lobby has loaded.
                    await Task.Delay(2_000, token).ConfigureAwait(false);

                    // Select the rental specified and minimize the game.
                    // Minimizing while a timer is active won't pause the timer.
                    var rental_adjust = Hub.Config.EncounterSWSH.MaxLairRentalToPick - 1;
                    for (var i = 0; i < rental_adjust; i++)
                        await Click(DDOWN, 0_500, token).ConfigureAwait(false);
                    await Click(A, 1_000, token).ConfigureAwait(false);
                    await Click(HOME, 1_600, token).ConfigureAwait(false);

                    if (await HandleEncounterMatchAction(msg!, token).ConfigureAwait(false))
                        return;

                    // If they choose not to keep it, we need to resume the game so CloseGame will work.
                    await Click(HOME, 1_600, token).ConfigureAwait(false);
                }

                Log("No match, resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);
            }
        }

        public async Task<(ulong s0, ulong s1)> GetMaxLairRNGState(ulong offset, bool log, CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, 16, token).ConfigureAwait(false);
            var s0 = BitConverter.ToUInt64(data, 0);
            var s1 = BitConverter.ToUInt64(data, 8);
            if (log)
                Log($"Lair Pokémon RNG state: {s0:x16}, {s1:x16}");
            return (s0, s1);
        }

        public async Task ClearMaxLairPenalty(CancellationToken token)
        {
            var data = BitConverter.GetBytes(0);
            await Connection.WriteBytesAsync(data, MaxLairPenaltyWarnOffset, token).ConfigureAwait(false);
            await Connection.WriteBytesAsync(data, MaxLairPenaltyCountOffset, token).ConfigureAwait(false);
        }

        public (bool match, string? print) IsMatchMaxLairLegendary(ulong s0, ulong s1)
        {
            // This should be the RNG state after generating 3 rental Pokémon.
            // Advance it 3 more times for replacement rentals, then 10 times for the Pokémon on the field.
            var rng_upper = new Xoroshiro128Plus(s0, s1);
            for (int i = 0; i < 13; i++)
                rng_upper.Next();

            // Generate the seed for the legendary Pokémon.
            var init = rng_upper.Next();
            var rng = new Xoroshiro128Plus(init);

            rng.NextInt(); // EC
            rng.NextInt(); // TID
            rng.NextInt(); // PID

            // Max Lair always has 4 fixed IVs.
            Span<int> ivs = [-1, -1, -1, -1, -1, -1];
            for (int i = 0; i < 4; i++)
            {
                int slot;
                do
                {
                    slot = (int)rng.NextInt(6);
                } while (ivs[slot] != -1);

                // Early return if greater than the max IV specified.
                if (DesiredMaxIVs[slot] < 31)
                {
                    Log($"{StatNames[slot]} IV is 31 and target max is {DesiredMaxIVs[slot]}.");
                    return (false, null);
                }

                ivs[slot] = 31;
            }
            for (int i = 0; i < 6; i++)
            {
                if (ivs[i] != -1)
                    continue;

                int iv = (int)rng.NextInt(32);

                // Early return if IVs out of range.
                if (iv < DesiredMinIVs[i] || iv > DesiredMaxIVs[i])
                {
                    Log($"{StatNames[i]} IV is {iv} and target range is {DesiredMinIVs[i]}-{DesiredMaxIVs[i]}.");
                    return (false, null);
                }

                ivs[i] = iv;
            }

            // Skip Ability -- all fixed.
            // Skip Gender -- all fixed.

            int nature = (int)rng.NextInt(25);
            if (Hub.Config.StopConditions.TargetNature != Nature.Random && nature != (int)Hub.Config.StopConditions.TargetNature)
            {
                Log($"Nature is {GameInfo.GetStrings("en").Natures[nature]} and target is {GameInfo.GetStrings("en").Natures[(int)Hub.Config.StopConditions.TargetNature]}.");
                return (false, null);
            }

            var msg = $"IVs: {ivs[0]}/{ivs[1]}/{ivs[2]}/{ivs[3]}/{ivs[4]}/{ivs[5]}, Nature: {GameInfo.GetStrings("en").Natures[nature]}";
            return (true, msg);
        }
    }
}
