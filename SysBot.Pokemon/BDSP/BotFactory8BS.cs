using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory8BS : BotFactory<PB8>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PB8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Dump
            => new PokeTradeBotBS(Hub, cfg),

        PokeRoutineType.EncBotTIDBS => new EncounterBotTIDBS(cfg, Hub),
        PokeRoutineType.EncBotZoneIDBS => new EncounterBotZoneIDBS(cfg, Hub),
        PokeRoutineType.EncBotCopySeedBS => new EncounterBotCopySeedBS(cfg, Hub),
        PokeRoutineType.EncBotRNGMonitorBS => new EncounterBotRNGMonitorBS(cfg, Hub),
        PokeRoutineType.EncBotDexFlipBS => new EncounterBotDexFlipBS(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotBS(cfg),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Dump
            => true,

        PokeRoutineType.EncBotTIDBS => true,
        PokeRoutineType.EncBotZoneIDBS => true,
        PokeRoutineType.EncBotCopySeedBS => true,
        PokeRoutineType.EncBotRNGMonitorBS => true,
        PokeRoutineType.EncBotDexFlipBS => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
