using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Pokemon.PokeDataOffsetsLGPE;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotCoordinatesLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : EncounterBotLGPE(cfg, hub)
    {

        // Cached offset to reduce pointer evaluation.
        private ulong CoordinatesOffset;
        float x;
        float y;
        float z;

        private bool CoordOutOfRange() => x > 100000 || z > 100000 || y > 100000 || x < -100000 || z < -100000 || y < -100000 || ((int)x == 0 && (int)z == 0 && (int)y == 0);

        protected override async Task EncounterLoop(SAV7b sav, CancellationToken token)
        {
            await UpdateCoordinatesPointer(token).ConfigureAwait(false);

            float prev_x = 0;
            float prev_z = 0;
            float prev_y = 0;
            var tries = 0;

            while (!token.IsCancellationRequested)
            {
                // If we've failed to print after 50 checks, reset the pointer. Handles transitions better.
                if (++tries >= 50)
                    await UpdateCoordinatesPointer(token).ConfigureAwait(false);

                await FetchCoordinates(token).ConfigureAwait(false);

                // Not a perfect check but these values occur during zone changes.
                while (CoordOutOfRange())
                {
                    await UpdateCoordinatesPointer(token).ConfigureAwait(false);
                    await FetchCoordinates(token).ConfigureAwait(false);
                }

                if (x == prev_x && z == prev_z && y == prev_y)
                    continue;

                prev_x = x;
                prev_z = z;
                prev_y = y;

                Log($"x: {x:F4}, y: {y:F4}, z: {z:F4}");
                tries = 0;
            }
        }

        private async Task UpdateCoordinatesPointer(CancellationToken token)
        {
            await SetMainLoopSleepTime(50, token).ConfigureAwait(false);
            bool valid = false;
            while (!valid)
                (valid, CoordinatesOffset) = await ValidatePointerAll(LGPECoordinatesPointer, token).ConfigureAwait(false);
            await SetMainLoopSleepTime(0, token).ConfigureAwait(false);
        }

        private async Task FetchCoordinates(CancellationToken token)
        {
            var data = await SwitchConnection.ReadBytesAbsoluteAsync(CoordinatesOffset, 12, token).ConfigureAwait(false);
            x = BitConverter.ToSingle(data, 0);
            z = BitConverter.ToSingle(data, 4);
            y = BitConverter.ToSingle(data, 8);
        }
    }
}
