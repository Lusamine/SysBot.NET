using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsFRLG;

namespace SysBot.Pokemon;

/// <summary>
/// Executor for FR/LG games.
/// </summary>
public abstract class PokeRoutineExecutor3FRLG(PokeBotState Config) : PokeRoutineExecutor<PK3>(Config)
{
    protected PokeDataOffsetsFRLG Offsets { get; } = new();

    public override Task<PK3> ReadPokemon(ulong offset, CancellationToken token) => ReadPokemon(offset, BoxFormatSlotSize, token);

    public override async Task<PK3> ReadPokemon(ulong offset, int size, CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync((uint)offset, size, token).ConfigureAwait(false);
        return new PK3(data);
    }

    public async Task<PK3> ReadPokemonAbsolute(ulong offset, int size, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAbsoluteAsync(offset, size, token).ConfigureAwait(false);
        return new PK3(data);
    }

    public override Task<PK3> ReadPokemonPointer(IEnumerable<long> jumps, int size, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public override Task<PK3> ReadBoxPokemon(int box, int slot, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public async Task<bool> ReadIsChanged(uint offset, byte[] original, CancellationToken token)
    {
        var result = await Connection.ReadBytesAsync(offset, original.Length, token).ConfigureAwait(false);
        return !result.SequenceEqual(original);
    }

    public async Task<SAV3FRLG> IdentifyTrainer(CancellationToken token)
    {
        // Check if botbase is on the correct version or later.
        await VerifyBotbaseVersion(token).ConfigureAwait(false);

        // Check title so we can warn if mode is incorrect.
        string title = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);
        if (!LanguageVersionOffsetsFRLG.IsTitleIDValidFRLG(title))
            throw new Exception($"{title} is not a valid FRLG title. Is your mode correct?");

        // Verify the game version.
        var game_version = await SwitchConnection.GetGameInfo("version", token).ConfigureAwait(false);
        if (!game_version.SequenceEqual(FRLGGameVersion))
            throw new Exception($"Game version is not supported. Expected version {FRLGGameVersion}, and current game version is {game_version}.");

        Log("Grabbing trainer data of host console...");
        var sav = await GetFakeTrainerSAV(title, token).ConfigureAwait(false);
        InitSaveData(sav);

        if (!IsValidTrainerData())
        {
            await CheckForRAMShiftingApps(token).ConfigureAwait(false);
            throw new Exception("Refer to the SysBot.NET wiki (https://github.com/kwsch/SysBot.NET/wiki/Troubleshooting) for more information.");
        }

        if ((TextSpeedOption)sav.SmallBlock.TextSpeed < TextSpeedOption.Fast)
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
    public async Task<SAV3FRLG> GetFakeTrainerSAV(string titleID, CancellationToken token)
    {
        var sav = new SAV3FRLG();
        var block = sav.SmallBuffer;
        var small_offset = await GetSmallOffset(titleID, token).ConfigureAwait(false);
        if (small_offset == 0)
            throw new Exception("Failed to get small block offset. Is your game version supported?");
        var read = await Connection.ReadBytesAsync(small_offset, sav.Small.Length, token).ConfigureAwait(false);
        read.CopyTo(block);
        sav.Language = (int)LanguageVersionOffsetsFRLG.GetLanguageFromTitleID(titleID);
        return sav;
    }

    public async Task<uint> GetSmallOffset(string titleID, CancellationToken token)
    {
        var current_seed_offset = LanguageVersionOffsetsFRLG.GetCurrentSeedOffsetFromTitleID(titleID);
        if (current_seed_offset == 0)
            return 0;
        var small = current_seed_offset + SmallShift; // Pointer to small block.
        var data = await Connection.ReadBytesAsync(small, 4, token).ConfigureAwait(false);
        var small_offset = BitConverter.ToUInt32(data, 0); // This offset is relative to 0x02020000, which is where InitialSeed is.
        if (small_offset == 0)
            return 0; // In case it's not loaded.
        return InitialSeed + (small_offset - 0x02020000); // Convert to absolute offset in the heap.        
    }

    public async Task<uint> GetLargeOffset(string titleID, CancellationToken token)
    {
        var current_seed_offset = LanguageVersionOffsetsFRLG.GetCurrentSeedOffsetFromTitleID(titleID);
        if (current_seed_offset == 0)
            return 0;
        var large = current_seed_offset + LargeShift; // Pointer to large block.
        var data = await Connection.ReadBytesAsync(large, 4, token).ConfigureAwait(false);
        var large_offset = BitConverter.ToUInt32(data, 0); // This offset is relative to 0x02020000, which is where InitialSeed is.
        if (large_offset == 0)
            return 0; // In case it's not loaded.
        return InitialSeed + (large_offset - 0x02020000); // Convert to absolute offset in the heap.
    }

    public async Task<uint> GetBoxStartOffset(string titleID, CancellationToken token)
    {
        var current_seed_offset = LanguageVersionOffsetsFRLG.GetCurrentSeedOffsetFromTitleID(titleID);
        if (current_seed_offset == 0)
            return 0;
        var boxes = current_seed_offset + BoxStartShift; // Pointer to box data block.
        var data = await Connection.ReadBytesAsync(boxes, 4, token).ConfigureAwait(false);
        var box_start_offset = BitConverter.ToUInt32(data, 0); // This offset is relative to 0x02020000, which is where InitialSeed is.
        if (box_start_offset == 0)
            return 0; // In case it's not loaded.
        return InitialSeed + (box_start_offset - 0x02020000); // Convert to absolute offset in the heap.
    }

    public async Task<Roamer3> GetRoamerData(string titleID, CancellationToken token)
    {
        var roamer_offset = await GetLargeOffset(titleID, token).ConfigureAwait(false) + 0x30D0;
        var data = await Connection.ReadBytesAsync(roamer_offset, 0x14, token).ConfigureAwait(false);
        return new Roamer3(data, IsGlitched: true);
    }

    public async Task SoftResetGame(uint offset, int extratime, CancellationToken token)
    {
        await PressAndHold([A, B, X, Y], 0_500, 0_500, token).ConfigureAwait(false);

        // Wait random amount of additional time before hitting any buttons.
        if (extratime > 0)
        {
            var extra = Util.Rand.Next(0, extratime);
            await Task.Delay(extra, token).ConfigureAwait(false);
            Log($"Waiting an extra {extra}ms before encountering to increase RNG variability.");
        }

        while (!await IsOnOverworld(offset, token).ConfigureAwait(false))
        {
            var button = Util.Rand.Next(2) == 0 ? A : X;
            var delay = Util.Rand.Next(0_200, 0_500);
            await Click(button, delay, token).ConfigureAwait(false);
        }

        // The overworld check becomes 0xFF while showing the journal replays, so press B to speed it up.
        for (int i = 0; i < 5; i++)
            await Click(B, 0_200, token).ConfigureAwait(false);

        Log("Back in the overworld!");
    }

    public async Task<bool> IsInBattle(CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAsync(InBattleOffset, 1, token).ConfigureAwait(false);
        return data[0] == 0x2;
    }

    public async Task<bool> IsOnOverworld(uint offset, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAsync(offset, 1, token).ConfigureAwait(false);
        return data[0] == 0xFF;
    }

    // Used to check if the battle menu has loaded, so we can attempt to flee.
    // This value is 0 as a battle starts and becomes 2 once the menu is loaded.
    public async Task<bool> IsOnBattleMenu(CancellationToken token)
    {
        var data = await Connection.ReadBytesAsync(BattleMenuOffset, 1, token).ConfigureAwait(false);
        return data[0] == 2;
    }

    public async Task<int> GetPartyCount(CancellationToken token)
    {
        var titleid = await SwitchConnection.GetTitleID(token).ConfigureAwait(false);
        var offset = LanguageVersionOffsetsFRLG.GetPartyStartOffsetFromTitleID(titleid);

        uint count = 0;
        for (uint i = 0; i < 6; i++)
        {
            var data = await SwitchConnection.ReadBytesAsync(offset + (i * 0x64), 1, token).ConfigureAwait(false);
            if (data[0] != 0)
                count++;
        }
        return (int)count;
    }

    public async Task<ushort> GetInitialRNGState(bool log, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAsync(InitialSeed, 2, token).ConfigureAwait(false);
        var seed = BitConverter.ToUInt16(data, 0);
        if (log)
            Log($"Initial RNG state: {seed:x4}");
        return seed;
    }

    public async Task<uint> GetCurrentRNGState(uint offset, bool log, CancellationToken token)
    {
        var data = await SwitchConnection.ReadBytesAsync(offset, 4, token).ConfigureAwait(false);
        var seed = BitConverter.ToUInt32(data, 0);
        if (log)
            Log($"RNG state: {seed:x8}");
        return seed;
    }
}
