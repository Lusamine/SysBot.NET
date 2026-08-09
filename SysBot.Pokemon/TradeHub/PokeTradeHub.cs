using PKHeX.Core;

namespace SysBot.Pokemon;

/// <summary>
/// Centralizes logic for trade bot coordination.
/// </summary>
/// <typeparam name="T">Type of <see cref="PKM"/> to distribute.</typeparam>
public class PokeTradeHub<T> where T : PKM, new()
{
    public PokeTradeHub(PokeTradeHubConfig config)
    {
        Config = config;
    }

    public readonly PokeTradeHubConfig Config;

}
