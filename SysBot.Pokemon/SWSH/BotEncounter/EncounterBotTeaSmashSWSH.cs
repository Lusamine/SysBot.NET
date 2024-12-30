using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotTeaSmashSWSH(PokeBotState cfg, PokeTradeHub<PK8> hub) : EncounterBotSWSH(cfg, hub)
    {
        protected override async Task EncounterLoop(SAV8SWSH sav, CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                PK8? pknew;
                var streak = 0;

                while (true)
                {
                    // If this isn't the first mon, see if an Authentic Sinistea spawned.
                    if (streak > 0)
                    {
                        // Prefer to use one read in case of something spawning in between.
                        var data = await Connection.ReadBytesAsync(LastSpeciesSpawned, 4, token).ConfigureAwait(false);
                        var lastspeciesform = BitConverter.ToUInt32(data, 0);

                        if (lastspeciesform != 0x10356)
                        {
                            var speciesfound = lastspeciesform & 0xffff;
                            var formfound = lastspeciesform >> 16;
                            var formfoundstring = formfound == 0 ? "" : "-" + formfound;
                            Log($"Found {(Species)speciesfound}{formfoundstring} instead.");
                            break;
                        }

                        // Whistle.
                        await Click(LSTICK, 3_000, token).ConfigureAwait(false);
                    }

                    // Walk forward a little to encounter the mon.
                    await SetStick(LEFT, 0, 30000, 1_100, token).ConfigureAwait(false);
                    await ResetStick(token).ConfigureAwait(false);

                    pknew = await ReadUntilPresent(WildPokemonOffset, 5_000, 0_200, BoxFormatSlotSize, token).ConfigureAwait(false);

                    // Sometimes it spawns slightly out of reach. Walk back and forth whistling.
                    if (pknew == null)
                    {
                        for (var tries = 5; tries > 0; tries--)
                        {
                            Log($"Failed to find Sinistea, looking around - attempt {6 - tries}.");
                            await FindTea(tries, token).ConfigureAwait(false);

                            pknew = await ReadUntilPresent(WildPokemonOffset, 0_200, 0_200, BoxFormatSlotSize, token).ConfigureAwait(false);
                            if (pknew != null)
                                break;
                        }
                        // Couldn't find it;
                        if (pknew == null)
                            break;
                    }

                    if (streak > 0 && await HandleEncounter(pknew, token).ConfigureAwait(false))
                        return;
                    if (pknew.Species != (int)Species.Sinistea || pknew.Form != 1)
                        break;

                    // We assume the first move will KO it. Press A until we get to overworld.
                    Log($"KOing Sinistea #{streak}...");
                    while (!await IsOnOverworld(OverworldOffset, token))
                        await Click(A, 0_050, token).ConfigureAwait(false);
                    streak++;

                    // Walk backward a little and edge up against the side. A presses are to get out of any Repel screens.
                    await SetStick(LEFT, 30000, 0, 0_500, token).ConfigureAwait(false);
                    await SetStick(LEFT, 0, -30000, 2_000, token).ConfigureAwait(false);
                    await Click(A, 0_050, token).ConfigureAwait(false);
                    await SetStick(LEFT, 0, 30000, 1_000, token).ConfigureAwait(false);
                    await ResetStick(token).ConfigureAwait(false);
                    await Click(A, 0_050, token).ConfigureAwait(false);

                    // Angle the camera.
                    await SetStick(RIGHT, -30000, 0, 0_050, token).ConfigureAwait(false);
                    await SetStick(RIGHT, 0, 0, 0_050, token).ConfigureAwait(false); // reset
                    await Task.Delay(1_700, token).ConfigureAwait(false);
                }

                Connection.Log($"Streak ended at {streak - 1}. Resetting the game...");
                await CloseGame(Hub.Config, token).ConfigureAwait(false);
                await StartGame(Hub.Config, token).ConfigureAwait(false);
                await Task.Delay(0_500, token).ConfigureAwait(false);
            }
        }

        private async Task FindTea(int attempts, CancellationToken token)
        {
            var direction = attempts % 2 == 0 ? -1 : 1;
            // Diagonally.
            await SetStick(LEFT, (short)(direction * 10_000), -30_000, 1_000, token).ConfigureAwait(false);
            if (attempts == 1 || attempts == 3)
            {
                await SetStick(LEFT, 0, -30000, 2_000, token).ConfigureAwait(false);
                await SetStick(LEFT, -30000, 0, 0_500, token).ConfigureAwait(false);
            }
            await ResetStick(token).ConfigureAwait(false);
            await Click(LSTICK, 2_000, token).ConfigureAwait(false);

            // Up
            if (attempts == 2)
                await SetStick(LEFT, 0, -30000, 1_000, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 30_000, 1_100, token).ConfigureAwait(false);
            await ResetStick(token).ConfigureAwait(false);
            await Click(A, 0_050, token).ConfigureAwait(false);  // In case of repels.
            await Click(LSTICK, 2_000, token).ConfigureAwait(false);
        }
    }
}
