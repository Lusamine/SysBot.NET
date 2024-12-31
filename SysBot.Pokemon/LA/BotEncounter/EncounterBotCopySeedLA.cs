using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCopySeedLA(PokeBotState cfg, PokeTradeHub<PA8> hub) : EncounterBotLA(cfg, hub)
    {
        private ulong MainRNGOffset;

        protected override async Task EncounterLoop(SAV8LA sav, CancellationToken token)
        {
            MainRNGOffset = await SwitchConnection.PointerAll(Offsets.MainRNGPointer, token).ConfigureAwait(false);
            var (s0, s1) = await GetGlobalRNGState(MainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedOutput(s0, s1, Hub.Config.EncounterLA.DisplaySeedMode);
            Log($"Copying global RNG state to clipboard:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
            CopyToClipboard(output);
        }
    }
}
