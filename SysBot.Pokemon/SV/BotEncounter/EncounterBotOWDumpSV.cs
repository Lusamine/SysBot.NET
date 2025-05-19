using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using System;
using SysBot.Base;
using System.IO;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotOWDumpSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : EncounterBotSV(cfg, hub)
    {
        /// <summary>
        /// Folder to dump oubreak blocks into.
        /// </summary>
        /// <remarks>If null, will skip dumping.</remarks>
        private readonly IDumper DumpSetting = hub.Config.Folder;

        // Save block keys
        private const uint KOverworld = 0x173304D8;
        private const uint KCoordinates = 0x708D1511;

        // Offsets to cache to reduce reads.
        private static ulong BlockKeyOffset;
        private static ulong OverworldOffset;
        private static ulong CoordinatesOffset;

        protected override async Task EncounterLoop(SAV9SV sav, CancellationToken token)
        {
            // Check if they've been initialized already.
            if (BlockKeyOffset == 0)
                await InitializeSessionOffsets(token).ConfigureAwait(false);

            var overworld_data = await GetOutbreakBlockValueArray(OverworldOffset, KOverworld, token).ConfigureAwait(false);

            var coords_data = await GetOutbreakBlockValueArray(CoordinatesOffset, KCoordinates, token).ConfigureAwait(false);
            var coords_hex = FetchCoordinatesHex(coords_data);

            var export_name = $"{coords_hex[0]:X8}-{coords_hex[1]:X8}-{coords_hex[2]:X8}-{sav.OT}";

            DumpBlock(DumpSetting.DumpFolder, "blocks", $"{export_name}.bin", overworld_data);
            Log("Done!");
            return;
        }

        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            BlockKeyOffset = await SwitchConnection.PointerAll(Offsets.BlockKeyStructPointer, token).ConfigureAwait(false);
            OverworldOffset = await GetKeyOffset(BlockKeyOffset, KOverworld, token).ConfigureAwait(false);
            CoordinatesOffset = await GetKeyOffset(BlockKeyOffset, KCoordinates, token).ConfigureAwait(false);
        }

        private async Task<byte[]> GetOutbreakBlockValueArray(ulong offset, uint key, CancellationToken token)
        {
            var (address, size) = await ReadKeyData(offset, key, token).ConfigureAwait(false);
            return await GetValueFromBlockKeyOffset(address, size, key, token).ConfigureAwait(false);
        }

        private static uint[] FetchCoordinatesHex(byte[] data)
        {
            var coords = new uint[3];
            coords[0] = BitConverter.ToUInt32(data, 0);
            coords[1] = BitConverter.ToUInt32(data, 4);
            coords[2] = BitConverter.ToUInt32(data, 8);
            return coords;
        }

        private static void DumpBlock(string folder, string subfolder, string filename, byte[] data)
        {
            if (!Directory.Exists(folder))
                return;
            var dir = Path.Combine(folder, subfolder);
            Directory.CreateDirectory(dir);
            var fn = Path.Combine(dir, filename);
            File.WriteAllBytes(fn, data);
            LogUtil.LogInfo($"Saved file: {fn}", "Dump");
        }
    }
}
