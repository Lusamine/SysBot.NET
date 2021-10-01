using PKHeX.Core;
using System;

namespace SysBot.Pokemon;

public sealed class BotFactory8SWSH : BotFactory<PK8>
{
    public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PK8> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.SurpriseTrade
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            or PokeRoutineType.SeedCheck
            => new PokeTradeBotSWSH(Hub, cfg),

        PokeRoutineType.RaidBot => new RaidBotSWSH(cfg, Hub),
        PokeRoutineType.EncBotLine => new EncounterBotLineSWSH(cfg, Hub),
        PokeRoutineType.EncBotEgg => new EncounterBotEggSWSH(cfg, Hub),
        PokeRoutineType.EncBotFossil => new EncounterBotFossilSWSH(cfg, Hub),
        PokeRoutineType.EncBotReset => new EncounterBotResetSWSH(cfg, Hub),
        PokeRoutineType.EncBotDog => new EncounterBotDogSWSH(cfg, Hub),
        PokeRoutineType.EncBotCamp => new EncounterBotCampSWSH(cfg, Hub),
        PokeRoutineType.EncBotFishing => new EncounterBotFishSWSH(cfg, Hub),
        PokeRoutineType.EncBotTeaSmash => new EncounterBotTeaSmashSWSH(cfg, Hub),
        PokeRoutineType.EncBotLairStatReset => new EncounterBotMaxLairStatResetSWSH(cfg, Hub),
        PokeRoutineType.EncBotCurryRNG => new EncounterBotCurryRNGSWSH(cfg, Hub),
        PokeRoutineType.EncBotCopySeed => new EncounterBotCopySeedSWSH(cfg, Hub),
        PokeRoutineType.EncBotRNGMonitor => new EncounterBotRNGMonitorSWSH(cfg, Hub),

        PokeRoutineType.RemoteControl => new RemoteControlBotSWSH(cfg),
        _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
    };

    public override bool SupportsRoutine(PokeRoutineType type) => type switch
    {
        PokeRoutineType.FlexTrade or PokeRoutineType.Idle
            or PokeRoutineType.SurpriseTrade
            or PokeRoutineType.LinkTrade
            or PokeRoutineType.Clone
            or PokeRoutineType.Dump
            or PokeRoutineType.SeedCheck
            => true,

        PokeRoutineType.RaidBot => true,
        PokeRoutineType.EncBotEgg => true,
        PokeRoutineType.EncBotFossil => true,
        PokeRoutineType.EncBotLine => true,
        PokeRoutineType.EncBotReset => true,
        PokeRoutineType.EncBotDog => true,
        PokeRoutineType.EncBotCamp => true,
        PokeRoutineType.EncBotFishing => true,
        PokeRoutineType.EncBotTeaSmash => true,
        PokeRoutineType.EncBotLairStatReset => true,
        PokeRoutineType.EncBotCurryRNG => true,
        PokeRoutineType.EncBotCopySeed => true,
        PokeRoutineType.EncBotRNGMonitor => true,

        PokeRoutineType.RemoteControl => true,

        _ => false,
    };
}
