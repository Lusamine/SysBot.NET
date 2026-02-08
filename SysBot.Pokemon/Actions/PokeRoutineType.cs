namespace SysBot.Pokemon;

/// <summary>
/// Type of routine the Bot carries out.
/// </summary>
public enum PokeRoutineType
{
    /// <summary> Sits idle waiting to be re-tasked. </summary>
    Idle = 0,

    /// <summary> Performs random trades using a predetermined pool of data. </summary>
    SurpriseTrade = 1,

    /// <summary> Performs the behavior of all trade bots. </summary>
    FlexTrade = 2,
    /// <summary> Performs only P2P Link Trades of specific data. </summary>
    LinkTrade = 3,
    /// <summary> Performs a seed check without transferring data from the bot. </summary>
    SeedCheck = 4,
    /// <summary> Performs a clone operation on the partner's data, sending them a copy of what they show. </summary>
    Clone = 5,
    /// <summary> Exports files for all data shown to the bot. </summary>
    Dump = 6,

    /// <summary> Performs group battles as a host. </summary>
    RaidBot = 7,

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

    /// <summary> Triggers encounters until the criteria is satisfied. </summary>
    EncBotResetLGPE = 10_000,

    /// <summary> Checks overworld spawns until the criteria is satisfied. </summary>
    EncBotRadarLGPE = 10_001,

    /// <summary> Uses RNG state to hunt for legendary birds in overworld. </summary>
    EncBotBirdWatchLGPE = 10_002,

    /// <summary> Prints float coordinates. </summary>
    EncBotCoordinatesLGPE = 10_003,

    /// <summary> Resets gifts and in-game trades. </summary>
    EncBotGiftLGPE = 10_004,

    /// <summary> Copies out the current global RNG state in the specified format. </summary>
    EncBotCopySeedLGPE = 10_005,

    /// <summary> Watch general RNG state. </summary>
    EncBotRNGMonitorLGPE = 10_006,

    /// <summary> Rolls dates and searches for outbreaks. </summary>
    EncBotOutbreakFinderSV = 10_007,

    /// <summary> Exports overworld blocks named as current coordinates. </summary>
    EncBotOWDumpSV = 10_008,

    /// <summary> Endlessly kills time at a bench. </summary>
    EncBotBenchLZA = 10_009,

    /// <summary> Assortment of simple macros for flying into Wild Zones. </summary>
    EncBotSimpleTricksLZA = 10_010,

    /// <summary> Presses A. </summary>
    EncBotASpamLZA = 10_011,

    /// <summary> Cycles the Pumpkaboo in Wild Zone 15. </summary>
    EncBotPumpkabooLZA = 10_012,

    /// <summary> Walk cycle for Virizion Special Scan. </summary>
    EncBotVirizionLZA = 10_013,
}

public static class PokeRoutineTypeExtensions
{
    public static bool IsTradeBot(this PokeRoutineType type) => type is >= PokeRoutineType.FlexTrade and <= PokeRoutineType.Dump;
    public static bool IsMonitorTool(this PokeRoutineType type) =>
        type is PokeRoutineType.EncBotCopySeed or PokeRoutineType.EncBotRNGMonitor
        or PokeRoutineType.EncBotCopySeedLGPE or PokeRoutineType.EncBotRNGMonitorLGPE
        or (>= PokeRoutineType.EncBotZoneIDBS and <= PokeRoutineType.EncBotRNGMonitorLA);
}
