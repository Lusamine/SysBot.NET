using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory8LA : BotFactory<PA8>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PA8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.EncBotOWLCheckRNGLA => new EncounterBotOWLCheckRNGLA(cfg, Hub),
        PokeRoutineType.EncBotCopySeedLA => new EncounterBotCopySeedLA(cfg, Hub),
        PokeRoutineType.EncBotRNGMonitorLA => new EncounterBotRNGMonitorLA(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotLA(cfg),

        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.EncBotOWLCheckRNGLA => true,
        PokeRoutineType.EncBotCopySeedLA => true,
        PokeRoutineType.EncBotRNGMonitorLA => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
