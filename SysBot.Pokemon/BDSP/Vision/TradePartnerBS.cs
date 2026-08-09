using PKHeX.Core;
using System;
using System.Diagnostics;

namespace SysBot.Pokemon.BDSP.Vision;

public sealed class TradePartnerBS
{
    public const int MaxByteLengthStringObject = 0x14 + 0x1A;

    public static string ReadStringFromRAMObject(byte[] obj)
    {
        // 0x10 typeinfo/monitor, 0x4 len, char[len]
        const int ofs_len = 0x10;
        const int ofs_chars = 0x14;
        Debug.Assert(obj.Length >= ofs_chars);

        // Detect string length, but be cautious about its correctness (protect against bad data)
        int maxCharCount = (obj.Length - ofs_chars) / 2;
        int length = BitConverter.ToInt32(obj, ofs_len);
        if (length < 0 || length > maxCharCount)
            length = maxCharCount;

        return StringConverter8.GetString(obj.AsSpan(ofs_chars, length * 2));
    }
}
