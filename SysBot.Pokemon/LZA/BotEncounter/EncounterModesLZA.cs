namespace SysBot.Pokemon;

public enum EncounterModeLZA
{
    /// <summary>
    /// Bot will repeatedly fly to Wild Zone 5's entrance to reset Froakie, alpha Ampharos, Flaaffy, Venipede, Falinks
    /// </summary>
    WildZone5LZA,

    /// <summary>
    /// Bot will repeatedly fly to Wild Zone 10's entrance to reset Slowpoke and Carvanha
    /// </summary>
    WildZone10LZA,

    /// <summary>
    /// Bot will repeatedly fly to Wild Zone 16's entrance to reset Falinks and Froakie
    /// </summary>
    WildZone16LZA,

    /// <summary>
    /// For use when raining. Bot will repeatedly enter the sewers to reset Goomy on the docks
    /// </summary>
    SewersRainLZA,
}
