using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory3FRLG : BotFactory<PK3>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK3> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.EncBotReset => new EncounterBotResetFRLG(cfg, Hub),
        PokeRoutineType.EncBotGCPrizeResetFRLG => new EncounterBotGCPrizeResetFRLG(cfg, Hub),
        PokeRoutineType.EncBotWildFRLG => new EncounterBotWildFRLG(cfg, Hub),
        PokeRoutineType.EncBotPickupFRLG => new EncounterBotPickupFRLG(cfg, Hub),
        PokeRoutineType.EncBotRoamerFRLG => new EncounterBotRoamerFRLG(cfg, Hub),
        PokeRoutineType.EncBotRNGMonitorFRLG => new EncounterBotRNGMonitorFRLG(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotFRLG(cfg),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.EncBotReset => true,
        PokeRoutineType.EncBotGCPrizeResetFRLG => true,
        PokeRoutineType.EncBotWildFRLG => true,
        PokeRoutineType.EncBotRoamerFRLG => true,
        PokeRoutineType.EncBotPickupFRLG => true,
        PokeRoutineType.EncBotRNGMonitorFRLG => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
