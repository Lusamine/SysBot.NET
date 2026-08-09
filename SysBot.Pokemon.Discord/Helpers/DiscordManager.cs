using System;
using System.Collections.Generic;
using System.Linq;

namespace SysBot.Pokemon.Discord;

public class DiscordManager(DiscordSettings Config)
{
    public readonly DiscordSettings Config = Config;
    public ulong Owner { get; internal set; }

    public RemoteControlAccessList WhitelistedChannels => Config.ChannelWhitelist;
    public RemoteControlAccessList SudoDiscord => Config.GlobalSudoList;
    public RemoteControlAccessList SudoRoles => Config.RoleSudo;
    public RemoteControlAccessList RolesRemoteControl => Config.RoleRemoteControl;

    public bool CanUseSudo(ulong uid) => SudoDiscord.Contains(uid);
    public bool CanUseSudo(IEnumerable<string> roles) => roles.Any(SudoRoles.Contains);

    public bool CanUseCommandChannel(ulong channel) => (WhitelistedChannels.List.Count == 0 && WhitelistedChannels.AllowIfEmpty) || WhitelistedChannels.Contains(channel);

    public bool GetHasRoleAccess(string type, IEnumerable<string> roles)
    {
        var set = GetSet(type);
        return set is { AllowIfEmpty: true, List.Count: 0 } || roles.Any(set.Contains);
    }

    private RemoteControlAccessList GetSet(string type) => type switch
    {
        nameof(RolesRemoteControl) => RolesRemoteControl,
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };
}
