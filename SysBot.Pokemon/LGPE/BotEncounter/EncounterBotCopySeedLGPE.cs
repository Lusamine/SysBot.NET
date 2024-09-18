using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    // This is for a specific RNG state that controls item spawning on the ground and other less useful
    // random processes like battle animations, follower Pokémon interactions, and field Pokémon movement.
    // If other RNG states need monitoring, this may be renamed in the future.
    public sealed class EncounterBotCopySeedLGPE : EncounterBotLGPE
    {
        private ulong GeneralRNGOffset;
        public EncounterBotCopySeedLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : base(cfg, hub)
        {
        }

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            GeneralRNGOffset = await SwitchConnection.PointerAll(LGPEGeneralRNGPointer, token).ConfigureAwait(false);
            var (s0, s1) = await GetGlobalRNGState(GeneralRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedOutput(s0, s1, Hub.Config.EncounterRNGBS.DisplaySeedMode);
            Log($"Copying global RNG state to clipboard:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
            CopyToClipboard(output);
        }
    }
}
