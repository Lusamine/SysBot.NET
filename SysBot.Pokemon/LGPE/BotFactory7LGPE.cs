using PKHeX.Core;
using System;

namespace SysBot.Pokemon
{
    public sealed class BotFactory7LGPE : BotFactory<PB7>
    {
        public override PokeRoutineExecutorBase CreateBot(PokeTradeHub<PB7> Hub, PokeBotState cfg) => cfg.NextRoutineType switch
        {
            PokeRoutineType.EncBotResetLGPE => new EncounterBotResetLGPE(cfg, Hub),
            PokeRoutineType.EncBotRadarLGPE => new EncounterBotRadarLGPE(cfg, Hub),
            PokeRoutineType.EncBotGiftLGPE => new EncounterBotGiftLGPE(cfg, Hub),
            PokeRoutineType.EncBotBirdWatchLGPE => new EncounterBotBirdWatchLGPE(cfg, Hub),
            PokeRoutineType.RemoteControl => new RemoteControlBotLGPE(cfg),
            PokeRoutineType.EncBotCoordinatesLGPE => new EncounterBotCoordinatesLGPE(cfg, Hub),

            _ => throw new ArgumentException(nameof(cfg.NextRoutineType)),
        };

        public override bool SupportsRoutine(PokeRoutineType type) => type switch
        {
            PokeRoutineType.EncBotResetLGPE or PokeRoutineType.EncBotRadarLGPE
                or PokeRoutineType.EncBotGiftLGPE
                or PokeRoutineType.EncBotBirdWatchLGPE
                or PokeRoutineType.EncBotCoordinatesLGPE
                => true,

            PokeRoutineType.RemoteControl => true,

            _ => false,
        };
    }
}
