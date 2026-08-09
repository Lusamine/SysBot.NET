using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SysBot.Pokemon;

public abstract class PokeRoutineExecutor<T>(IConsoleBotManaged<IConsoleConnection, IConsoleConnectionAsync> Config)
    : PokeRoutineExecutorBase(Config)
    where T : PKM, new()
{
    public abstract Task<T> ReadPokemon(ulong offset, CancellationToken token);
    public abstract Task<T> ReadPokemon(ulong offset, int size, CancellationToken token);
    public abstract Task<T> ReadPokemonPointer(IEnumerable<long> jumps, int size, CancellationToken token);
    public abstract Task<T> ReadBoxPokemon(int box, int slot, CancellationToken token);

    public async Task<T?> ReadUntilPresent(ulong offset, int waitms, int waitInterval, int size, CancellationToken token)
    {
        int msWaited = 0;
        while (msWaited < waitms)
        {
            var pk = await ReadPokemon(offset, size, token).ConfigureAwait(false);
            if (pk.Species != 0 && pk.ChecksumValid)
                return pk;
            await Task.Delay(waitInterval, token).ConfigureAwait(false);
            msWaited += waitInterval;
        }
        return null;
    }

    public async Task<T?> ReadUntilPresentPointer(IReadOnlyList<long> jumps, int waitms, int waitInterval, int size, CancellationToken token)
    {
        int msWaited = 0;
        while (msWaited < waitms)
        {
            var pk = await ReadPokemonPointer(jumps, size, token).ConfigureAwait(false);
            if (pk.Species != 0 && pk.ChecksumValid)
                return pk;
            await Task.Delay(waitInterval, token).ConfigureAwait(false);
            msWaited += waitInterval;
        }
        return null;
    }

    protected async Task<(bool, ulong)> ValidatePointerAll(IEnumerable<long> jumps, CancellationToken token)
    {
        var solved = await SwitchConnection.PointerAll(jumps, token).ConfigureAwait(false);
        return (solved != 0, solved);
    }

    public static void DumpPokemon(string folder, string subfolder, T pk)
    {
        if (!Directory.Exists(folder))
            return;
        var dir = Path.Combine(folder, subfolder);
        Directory.CreateDirectory(dir);
        var fn = Path.Combine(dir, PathUtil.CleanFileName(pk.FileName));
        Span<byte> data = stackalloc byte[pk.SIZE_PARTY];
        pk.WriteDecryptedDataParty(data);
        File.WriteAllBytes(fn, data);
        LogUtil.LogInfo($"Saved file: {fn}", "Dump");
    }

    public string GetSeedOutput(ulong s0, ulong s1, DisplaySeedMode mode)
    {
        string seed0 = $"{s0:x16}";
        string seed1 = $"{s1:x16}";

        return mode switch
        {
            DisplaySeedMode.Bit128 => $"{seed1}{seed0}",
            DisplaySeedMode.Bit64 => $"{seed0}{Environment.NewLine}{seed1}",
            DisplaySeedMode.Bit64PokeFinder => SplitSeed32Bit(seed0, seed1, mode),
            DisplaySeedMode.Bit32 => SplitSeed32Bit(seed0, seed1, mode),
            _ => $"{seed0}{Environment.NewLine}{seed1}",
        };
    }

    // Realistically, people will only be monitoring in s0/s1 or s0/s1/s2/s3 format.
    public string GetSeedMonitorOutput(ulong s0, ulong s1, DisplaySeedMode mode)
    {
        string seed0 = $"{s0:x16}";
        string seed1 = $"{s1:x16}";

        return mode switch
        {
            DisplaySeedMode.Bit128 => $"{seed1}{seed0}",
            DisplaySeedMode.Bit64 => $"{seed0} {seed1}",
            DisplaySeedMode.Bit32 => SplitSeed32Bit(seed0, seed1, mode, false),
            _ => $"s0: {seed0} s1: {seed1}",
        };
    }

    // Accommodate standard RNG tools splitting this into 4 seeds.
    public string SplitSeed32Bit(string seed0, string seed1, DisplaySeedMode mode, bool copy = true)
    {
        var _s0 = seed0[8..];
        var _s1 = seed0[..8];
        var _s2 = seed1[8..];
        var _s3 = seed1[..8];

        // Format for setting seeds in clipboard or for copying.
        if (copy)
        {
            if (mode is DisplaySeedMode.Bit32)
                return $"{_s0}{Environment.NewLine}{_s1}{Environment.NewLine}{_s2}{Environment.NewLine}{_s3}";
            return $"{_s0}{_s1}{Environment.NewLine}{_s2}{_s3}";
        }

        return $"{_s0} {_s1} {_s2} {_s3}";
    }

    public void CopyToClipboard(string output)
    {
        try
        {
            TextCopy.ClipboardService.SetText(output);
        }
        catch (Exception ex)
        {
            Log(ex.ToString());
        }
    }

    public async Task VerifyBotbaseVersion(CancellationToken token)
    {
        var data = await SwitchConnection.GetBotbaseVersion(token).ConfigureAwait(false);
        var version = System.Version.TryParse(data, out var v) ? v : null;
        if (version < BotbaseVersion || version is null)
        {
            var protocol = Config.Connection.Protocol;
            var msg = protocol is SwitchProtocol.WiFi ? "sys-botbase" : "usb-botbase";
            msg += $" version is not supported. Expected version {BotbaseVersion} or greater, and your current version is {data}. Please download the latest version from: ";
            if (protocol is SwitchProtocol.WiFi)
                msg += "https://github.com/olliz0r/sys-botbase/releases/latest";
            else
                msg += "https://github.com/Koi-3088/usb-botbase/releases/latest";
            throw new Exception(msg);
        }
    }

    // Check if either Tesla or dmnt are active if the sanity check for Trainer Data fails, as these are common culprits.
    private const ulong ovlloaderID = 0x420000000007e51a; // Tesla Menu
    private const ulong dmntID = 0x010000000000000d;      // dmnt used for cheats

    public async Task CheckForRAMShiftingApps(CancellationToken token)
    {
        Log("Trainer data is not valid.");

        bool found = false;
        var msg = "Found ";
        if (await SwitchConnection.IsProgramRunning(ovlloaderID, token).ConfigureAwait(false))
        {
            msg += "Tesla Menu";
            found = true;
        }

        if (await SwitchConnection.IsProgramRunning(dmntID, token).ConfigureAwait(false))
        {
            if (found)
                msg += " and ";
            msg += "dmnt (cheat codes?)";
            found = true;
        }

        if (found)
        {
            msg += ".";
            Log(msg);
            Log("Please remove interfering applications and reboot the Switch.");
        }
    }

    private static RemoteControlAccess GetReference(string name, ulong id, string comment) => new()
    {
        ID = id,
        Name = name,
        Comment = $"Added automatically on {DateTime.Now:yyyy.MM.dd-hh:mm:ss} ({comment})",
    };
}
