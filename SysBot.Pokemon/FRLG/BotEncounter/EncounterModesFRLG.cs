namespace SysBot.Pokemon;

public enum EncounterModeFRLG
{
    /// <summary>
    /// Bot will reset the starter from party slot 1.
    /// </summary>
    StarterFRLG,

    /// <summary>
    /// Bot will reset a gift from party slot 2.
    /// </summary>
    GiftFRLG,

    /// <summary>
    /// Bot will reset a wild static encounter Pokémon.
    /// </summary>
    StaticFRLG,

    /// <summary>
    /// Bot will hunt for a wild encounter slot Pokémon.
    /// </summary>
    SlotsFRLG,

    /// <summary>
    /// Bot will hunt for a fishing encounter slot Pokémon.
    /// </summary>
    FishingFRLG,
}
