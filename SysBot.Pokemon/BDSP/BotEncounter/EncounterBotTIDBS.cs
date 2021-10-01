using PKHeX.Core;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotTIDBS : EncounterBotBS
    {
        public readonly IReadOnlyList<string> TargetTIDBS;

        public EncounterBotTIDBS(PokeBotState cfg, PokeTradeHub<PB8> hub) : base(cfg, hub)
        {
            StopConditionSettings.ReadTargetTIDBS(Hub.Config.StopConditions, out TargetTIDBS);
        }

        private ulong MainRNGOffset;
        private readonly int threshold = 15;

        protected override async Task EncounterLoop(SAV8BS sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                MainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);

                if (Hub.Config.EncounterRNGBS.MaxTIDAdvances < 100)
                {
                    Log($"Please set advances greater than 100. Approximately 100 advances are required to reach character selection.");
                    return;
                }
                if (TargetTIDBS.Count == 0)
                {
                    Log("Please set target TIDs under Stop Conditions before starting the bot.");
                    return;
                }

                // Get the initial current RNG state.
                Log("Checking the RNG state...");
                var (s0, s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
                var (found, advances) = CheckForTID(s0, s1);
                if (!found)
                {
                    Log($"No match after {advances} advances, resetting the game...");
                    await CloseGame(Hub.Config, token).ConfigureAwait(false);
                    await StartGame(true, Hub.Config, token).ConfigureAwait(false);
                    continue;
                }

                // If you want it to select a different language, change the keypresses here.
                // It will pick the first option by default.
                Log("Accepting language.");
                await Click(A, 0_050, token).ConfigureAwait(false);
                await PressAndHold(A, 1_000, 0, token).ConfigureAwait(false);
                await Click(A, 0_500, token).ConfigureAwait(false);

                // This should get us to the name entry, and B's are free to press.
                Log("Going through Rowan's introduction.");
                for (int i = 0; i < 40; i++)
                {
                    await Click(B, 0_100, token).ConfigureAwait(false);
                    await PressAndHold(B, 0_500, 0, token).ConfigureAwait(false);
                }

                // If we found a match, advances should be the number of times we need to advance the game.
                Log("Advancing the RNG state...");
                while (!token.IsCancellationRequested)
                {
                    await Click(A, 1_500, token).ConfigureAwait(false);
                    await Click(A, 0_100, token).ConfigureAwait(false);
                    await Click(PLUS, 1_000, token).ConfigureAwait(false);
                    await Click(PLUS, 1_000, token).ConfigureAwait(false);
                    await Click(B, 1_000, token).ConfigureAwait(false);

                    // Check how many advances went by.
                    var (_s0, _s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
                    var passed = GetAdvancesPassed(s0, s1, _s0, _s1);
                    advances -= passed;

                    if (advances < threshold)
                    {
                        Log($"Stopping bot within {advances} advances of target TID.");
                        await Click(HOME, 2_000, token).ConfigureAwait(false);

                        var output = GetSeedOutput(_s0, _s1, Hub.Config.EncounterRNGBS.DisplaySeedMode);
                        Log($"Current global RNG state is:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
                        return;
                    }

                    // Store the state for the next pass.
                    s0 = _s0;
                    s1 = _s1;
                }
            }
        }

        private (bool found, int advances) CheckForTID(ulong s0, ulong s1)
        {
            var rng = new XorShift128(s0, s1);
            var advances = 0;
            var maxAdvances = Hub.Config.EncounterRNGBS.MaxTIDAdvances;

            for (; advances < maxAdvances; advances++)
            {
                var TID = GetTIDFromRNGState(rng, false, out XorShift128 newrng);
                if (Hub.Config.StopConditions.IsTargetTIDBS(TID, TargetTIDBS))
                {
                    Log($"Found a match for {TID} in {advances} advances.");
                    return (true, advances);
                }
                rng = newrng;
            }
            return (false, advances);
        }
    }
}
