using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCopySeedSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            var (s0, s1) = await GetGlobalRNGState(SWSHMainRNGOffset, false, token).ConfigureAwait(false);
            var output = GetSeedOutput(s0, s1, Hub.Config.EncounterSWSH.DisplaySeedMode);
            Log($"Copying global RNG state to clipboard:{Environment.NewLine}{Environment.NewLine}{output}{Environment.NewLine}");
            CopyToClipboard(output);
        }
    }
}
