using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsSWSH;

namespace SysBot.Pokemon;

/// <summary>
/// Executor for SW/SH games.
/// </summary>
public abstract class PokeRoutineExecutor8SWSH(PokeBotState Config) : PokeRoutineExecutor<PK8>(Config)
{
    protected PokeDataOffsetsSWSH Offsets { get; } = new();

    private static uint GetBoxSlotOffset(int box, int slot) => BoxStartOffset + (uint)(BoxFormatSlotSize * ((30 * box) + slot));

    public override Task<PK8> ReadPokemon(ulong offset, CancellationToken token) => ReadPokemon(offset, BoxFormatSlotSize, token);

    public override async Task<PK8> ReadPokemon(ulong offset, int size, CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync((uint)offset, size, token).ConfigureAwait(false);
        return new PK8(data);
    }

    public async Task<PK8> ReadPokemonAbsolute(ulong offset, int size, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, size, token).ConfigureAwait(false);
        return new PK8(data);
    }

    public override async Task<PK8> ReadPokemonPointer(IEnumerable<long> jumps, int size, CancellationToken token)
    {
        var (valid, offset) = await ValidatePointerAll(jumps, token).ConfigureAwait(false);
        if (!valid)
            return new PK8();
        return await ReadPokemonAbsolute(offset, size, token).ConfigureAwait(false);
    }

    public Task SetBoxPokemon(PK8 pkm, int box, int slot, CancellationToken token, ITrainerInfo? sav = null)
    {
        if (sav != null)
        {
            // Update PKM to the current save's handler data
            pkm.UpdateHandler(sav);
            pkm.RefreshChecksum();
        }
        var offset = GetBoxSlotOffset(box, slot);
        pkm.ResetPartyStats();
        Span<byte> data = stackalloc byte[pkm.SIZE_PARTY];
        pkm.WriteEncryptedDataParty(data);
        return SwitchConnection.WriteBytesAsync(data.ToArray(), offset, token);
    }

    public override Task<PK8> ReadBoxPokemon(int box, int slot, CancellationToken token)
    {
        var offset = GetBoxSlotOffset(box, slot);
        return ReadPokemon(offset, BoxFormatSlotSize, token);
    }

    public Task SetCurrentBox(byte box, CancellationToken token)
    {
        return Connection.WriteBytesAsync([box], CurrentBoxOffset, token);
    }

    public async Task<byte> GetCurrentBox(CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync(CurrentBoxOffset, 1, token).ConfigureAwait(false);
        return data[0];
    }

    public async Task<bool> ReadIsChanged(uint offset, byte[] original, CancellationToken token)
    {
        var result = await Connection.ReadBytesAsync(offset, original.Length, token).ConfigureAwait(false);
        return !result.SequenceEqual(original);
    }

    public async Task<SAV8SWSH> IdentifyTrainer(CancellationToken token)
    {
        // Check if botbase is on the correct version or later.
        await VerifyBotbaseVersion(token).ConfigureAwait(false);

        // Check title so we can warn if mode is incorrect.
        string title = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);
        if (title is not (SwordID or ShieldID))
            throw new Exception($"{title} is not a valid SWSH title. Is your mode correct?");

        // Verify the game version.
        var game_version = await SwitchConnection.GetGameInfo("version", token).ConfigureAwait(false);
        if (!game_version.SequenceEqual(SWSHGameVersion))
            throw new Exception($"Game version is not supported. Expected version {SWSHGameVersion}, and current game version is {game_version}.");

        Log("Grabbing trainer data of host console...");
        var sav = await GetFakeTrainerSAV(token).ConfigureAwait(false);
        InitSaveData(sav);

        if (!IsValidTrainerData())
        {
            await CheckForRAMShiftingApps(token).ConfigureAwait(false);
            throw new Exception("Refer to the SysBot.NET wiki (https://github.com/kwsch/SysBot.NET/wiki/Troubleshooting) for more information.");
        }

        if (await GetTextSpeed(token).ConfigureAwait(false) < TextSpeedOption.Fast)
            throw new Exception("Text speed should be set to FAST. Fix this for correct operation.");

        return sav;
    }

    public async Task InitializeHardware(IBotStateSettings settings, CancellationToken token)
    {
        Log("Detaching on startup.");
        await DetachController(token).ConfigureAwait(false);
        if (settings.ScreenOff)
        {
            Log("Turning off screen.");
            await SetScreen(ScreenState.Off, token).ConfigureAwait(false);
        }
    }

    public async Task CleanExit(CancellationToken token)
    {
        await SetScreen(ScreenState.On, token).ConfigureAwait(false);
        Log("Detaching controllers on routine exit.");
        await DetachController(token).ConfigureAwait(false);
    }

    /// <summary>
    /// Identifies the trainer information and loads the current runtime language.
    /// </summary>
    public async Task<SAV8SWSH> GetFakeTrainerSAV(CancellationToken token)
    {
        var sav = new SAV8SWSH();
        var info = sav.MyStatus;
        var read = await Connection.ReadBytesAsync(TrainerDataOffset, TrainerDataLength, token).ConfigureAwait(false);
        read.CopyTo(info.Data);
        return sav;
    }

    public async Task CloseGame(PokeTradeHubConfig config, CancellationToken token)
    {
        var timing = config.Timings;
        // Close out of the game
        await Click(HOME, 2_000 + timing.ExtraTimeReturnHome, token).ConfigureAwait(false);
        await Click(X, 1_000, token).ConfigureAwait(false);
        await Click(A, 5_000 + timing.ExtraTimeCloseGame, token).ConfigureAwait(false);
        Log("Closed out of the game!");
    }

    public async Task StartGame(PokeTradeHubConfig config, CancellationToken token)
    {
        var timing = config.Timings;
        // Open game.
        await Click(A, 1_000 + timing.ExtraTimeLoadProfile, token).ConfigureAwait(false);

        // Menus here can go in the order: Update Prompt -> Profile -> Starts Game
        // The user can optionally turn on the setting if they know of a breaking system update incoming.
        if (timing.AvoidSystemUpdate)
        {
            await Task.Delay(1_000, token).ConfigureAwait(false); // Reduce the chance of misclicking here.
            await Click(DUP, 0_600, token).ConfigureAwait(false);
            await Click(A, 1_000 + timing.ExtraTimeLoadProfile, token).ConfigureAwait(false);
        }

        await Click(A, 0_600, token).ConfigureAwait(false);

        Log("Restarting the game!");

        // Switch Logo lag, skip cutscene, game load screen
        await Task.Delay(10_000 + timing.ExtraTimeLoadGame, token).ConfigureAwait(false);

        for (int i = 0; i < 4; i++)
            await Click(A, 1_000, token).ConfigureAwait(false);

        var timer = 60_000;
        while (!await IsOnOverworldTitle(token).ConfigureAwait(false) && !await IsInBattle(token).ConfigureAwait(false))
        {
            await Task.Delay(0_200, token).ConfigureAwait(false);
            timer -= 0_250;
            // We haven't made it back to overworld after a minute, so press A every 6 seconds hoping to restart the game.
            // Don't risk it if hub is set to avoid updates.
            if (timer <= 0 && !timing.AvoidSystemUpdate)
            {
                Log("Still not in the game, initiating rescue protocol!");
                while (!await IsOnOverworldTitle(token).ConfigureAwait(false) && !await IsInBattle(token).ConfigureAwait(false))
                    await Click(A, 6_000, token).ConfigureAwait(false);
                break;
            }
        }

        Log("Back in the overworld!");
    }

    public async Task<bool> IsInBattle(CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync(Version == GameVersion.SH ? InBattleRaidOffsetSH : InBattleRaidOffsetSW, 1, token).ConfigureAwait(false);
        return data[0] == (Version == GameVersion.SH ? 0x40 : 0x41);
    }

    // Only used to check if we made it off the title screen.
    private async Task<bool> IsOnOverworldTitle(CancellationToken token)
    {
        var (valid, offset) = await ValidatePointerAll(Offsets.OverworldPointer, token).ConfigureAwait(false);
        if (!valid)
            return false;
        return await IsOnOverworld(offset, token).ConfigureAwait(false);
    }

    public async Task<bool> IsOnOverworld(ulong offset, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, 1, token).ConfigureAwait(false);
        return data[0] == 1;
    }

    // Used to check if the battle menu has loaded, so we can attempt to flee.
    // This value starts at 1 and goes up each time a menu is opened.
    public async Task<bool> IsOnBattleMenu(CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync(BattleMenuOffset, 1, token).ConfigureAwait(false);
        return data[0] >= 1;
    }

    public async Task<TextSpeedOption> GetTextSpeed(CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync(TextSpeedOffset, 1, token).ConfigureAwait(false);
        return (TextSpeedOption)(data[0] & 3);
    }

    public async Task SetTextSpeed(TextSpeedOption speed, CancellationToken token)
    {
        var textSpeedByte = await Connection.ReadBytesAsync(TextSpeedOffset, 1, token).ConfigureAwait(false);
        var data = new[] { (byte)((textSpeedByte[0] & 0xFC) | (int)speed) };
        await Connection.WriteBytesAsync(data, TextSpeedOffset, token).ConfigureAwait(false);
    }

    // Switches to box 1, then clears slot1 to prep fossil and egg bots.
    public async Task SetupBoxState(IDumper DumpSetting, CancellationToken token)
    {
        await SetCurrentBox(0, token).ConfigureAwait(false);

        var existing = await ReadBoxPokemon(0, 0, token).ConfigureAwait(false);
        if (existing.Species != 0 && existing.ChecksumValid)
        {
            Log("Destination slot is occupied! Dumping the Pokémon found there...");
            DumpPokemon(DumpSetting.DumpFolder, "saved", existing);
        }

        Log("Clearing destination slot to start the bot.");
        PK8 blank = new();
        await SetBoxPokemon(blank, 0, 0, token).ConfigureAwait(false);
    }

    public async Task<(ulong s0, ulong s1)> GetGlobalRNGState(uint offset, bool log, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAsync(offset, 16, token).ConfigureAwait(false);
        var s0 = BitConverter.ToUInt64(data, 0);
        var s1 = BitConverter.ToUInt64(data, 8);
        if (log)
            Log($"RNG state: {s0:x16}, {s1:x16}");
        return (s0, s1);
    }

    public static int GetAdvancesPassed(ulong prevs0, ulong prevs1, ulong news0, ulong news1)
    {
        if (prevs0 == news0 && prevs1 == news1)
            return 0;

        var rng = new Xoroshiro128Plus(prevs0, prevs1);
        for (int i = 0; ; i++)
        {
            rng.Next();
            var (s0, s1) = rng.GetState();
            if (s0 == news0 && s1 == news1)
                return i + 1;
        }
    }
}
