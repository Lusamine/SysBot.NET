namespace SysBot.Pokemon;

/// <summary>
/// Type of routine the Bot carries out.
/// </summary>
public enum PokeRoutineType
{
    /// <summary> Sits idle waiting to be re-tasked. </summary>
    Idle = 0,

    /// <summary> Triggers walking encounters until the criteria is satisfied. </summary>
    EncBotLine = 1_000,

    /// <summary> Triggers reset encounters until the criteria is satisfied. </summary>
    EncBotReset = 1_001,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotDog = 1_002,

    /// <summary> Retrieves eggs from the Day Care. </summary>
    EncBotEgg = 1_003,

    /// <summary> Revives fossils until the criteria is satisfied. </summary>
    EncBotFossil = 1_004,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotCamp = 1_005,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotFishing = 1_006,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotTeaSmash = 1_007,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotLairStatReset = 1_008,

    /// <summary> RNG abuses curry spawn until the criteria are satisfied </summary>
    EncBotCurryRNG = 1_009,

    /// <summary> RNG abuses tree spawn until the criteria are satisfied </summary>
    EncBotTreeRNG = 1_010,

    /// <summary> Copies out the current global RNG state in the specified format. </summary>
    EncBotCopySeed = 1_011,

    /// <summary> Prints out the global RNG state and information on advances passed. </summary>
    EncBotRNGMonitor = 1_012,

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotTIDBS = 2_000,

    /// <summary> Continously prints the player character's current zoneID. </summary>
    EncBotZoneIDBS = 2_001,

    /// <summary> Copies out the current global RNG state in the specified format. </summary>
    EncBotCopySeedBS = 2_002,

    /// <summary> Prints out the global RNG state and information on advances passed. </summary>
    EncBotRNGMonitorBS = 2_003,

    /// <summary> Flips the Poké Dex to advance the RNG state. </summary>
    EncBotDexFlipBS = 2_004,

    /// <summary> Copies out the current global RNG state in the specified format. </summary>
    EncBotCopySeedLA = 3_000,

    /// <summary> Prints out the global RNG state and information on advances passed. </summary>
    EncBotRNGMonitorLA = 3_001,

    /// <summary> Checks for overworld legend seeds for RNG manipulation. </summary>
    EncBotOWLCheckRNGLA = 3_002,

    /// <summary> Similar to idle, but identifies the bot as available for Remote input (Twitch Plays, etc.). </summary>
    RemoteControl = 6_000,

    // Add your own custom bots here so they don't clash for future main-branch bot releases.

    /// <summary> Checks overworld spawns until the criteria is satisfied. </summary>
    EncBotRadarLGPE = 10_000,

    /// <summary> Uses RNG state to hunt for legendary birds in overworld. </summary>
    EncBotBirdWatchLGPE = 10_001,

    /// <summary> Prints float coordinates. </summary>
    EncBotCoordinatesLGPE = 10_002,

    /// <summary> Resets gifts and in-game trades. </summary>
    EncBotGiftLGPE = 10_003,

    /// <summary> Copies out the current global RNG state in the specified format. </summary>
    EncBotCopySeedLGPE = 10_004,

    /// <summary> Watch general RNG state. </summary>
    EncBotRNGMonitorLGPE = 10_005,

    /// <summary> Rolls dates and searches for outbreaks. </summary>
    EncBotOutbreakFinderSV = 11_000,

    /// <summary> Exports overworld blocks named as current coordinates. </summary>
    EncBotOWDumpSV = 11_001,

    /// <summary> Endlessly kills time at a bench. </summary>
    EncBotBenchLZA = 12_000,

    /// <summary> Assortment of simple macros for flying into Wild Zones. </summary>
    EncBotSimpleTricksLZA = 12_001,

    /// <summary> Presses A. </summary>
    EncBotASpamLZA = 12_002,

    /// <summary> Cycles the Pumpkaboo in Wild Zone 15. </summary>
    EncBotPumpkabooLZA = 12_003,

    /// <summary> Walk cycle for Virizion Special Scan. </summary>
    EncBotVirizionLZA = 12_004,

    /// <summary> Prints out the global RNG state and information on advances passed. </summary>
    EncBotRNGMonitorFRLG = 13_000,

    /// <summary> Buys 1-5 prizes from the Game Corner. </summary>
    EncBotGCPrizeResetFRLG = 13_001,

    /// <summary> Wiggles in place to encounter wild Pokémon. </summary>
    EncBotWildFRLG = 13_002,

    /// <summary> Resets a roaming Pokémon. </summary>
    EncBotRoamerFRLG = 13_003,

    /// <summary> Runs a Pickup party in Lavender Tower. </summary>
    EncBotPickupFRLG = 13_004,
}

public static class PokeRoutineTypeExtensions
{
    public static bool IsMonitorTool(this PokeRoutineType type) =>
        type is PokeRoutineType.EncBotCopySeed or PokeRoutineType.EncBotRNGMonitor
        or PokeRoutineType.EncBotCopySeedLGPE or PokeRoutineType.EncBotRNGMonitorLGPE
        or (>= PokeRoutineType.EncBotZoneIDBS and <= PokeRoutineType.EncBotRNGMonitorLA);
}
