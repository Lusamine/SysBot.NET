using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory9LZA : BotFactory<PA9>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PA9> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            => new PokeTradeBotLZA(Hub, cfg),

        PokeRoutineType.EncBotSimpleTricksLZA => new EncounterBotSimpleTricksLZA(cfg, Hub),
        PokeRoutineType.EncBotBenchLZA => new EncounterBotBenchLZA(cfg, Hub),
        PokeRoutineType.EncBotReset => new EncounterBotResetLZA(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotLZA(cfg),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            => true,

        PokeRoutineType.EncBotSimpleTricksLZA => true,
        PokeRoutineType.EncBotBenchLZA => true,
        PokeRoutineType.EncBotReset => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
