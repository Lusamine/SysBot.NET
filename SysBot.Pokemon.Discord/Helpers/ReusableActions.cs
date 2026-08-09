using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SysBot.Pokemon.Discord;

public static class ReusableActions
{
    public static async Task EchoAndReply(this ISocketMessageChannel channel, string msg)
    {
        // Announce it in the channel the command was entered only if it's not already an echo channel.
        EchoUtil.Echo(msg);
        if (!EchoModule.IsEchoChannel(channel))
            await channel.SendMessageAsync(msg).ConfigureAwait(false);
    }

    public static string GetFormattedShowdownText(PKM pkm)
    {
        var showdown = ShowdownParsing.GetShowdownText(pkm);
        return Format.Code(showdown);
    }

    private static readonly string[] separator = [ ",", ", ", " " ];

    public static IReadOnlyList<string> GetListFromString(string str)
    {
        // Extract comma separated list
        return str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string StripCodeBlock(string str) => str
        .Replace("`\n", "")
        .Replace("\n`", "")
        .Replace("`", "")
        .Trim();
}
