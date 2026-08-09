using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory9SV : BotFactory<PK9>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK9> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.EncBotOutbreakFinderSV => new EncounterBotOutbreakFinderSV(cfg, Hub),
        PokeRoutineType.EncBotOWDumpSV => new EncounterBotOWDumpSV(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotSV(cfg),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.EncBotOutbreakFinderSV => true,
        PokeRoutineType.EncBotOWDumpSV => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
