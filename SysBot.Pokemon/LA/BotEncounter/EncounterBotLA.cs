﻿using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public abstract class EncounterBotLA : PokeRoutineExecutor8LA, IEncounterBot
    {
        protected readonly PokeTradeHub<PA8> Hub;
        private readonly EncounterRNGBSSettings Settings;
        public ICountSettings Counts => Settings;

        protected EncounterBotLA(PokeBotState cfg, PokeTradeHub<PA8> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.EncounterRNGBS;
        }

        public override async Task MainLoop(CancellationToken token)
        {
            var settings = Hub.Config.EncounterRNGBS;
            Log("Identifying trainer data of the host console.");
            var sav = await IdentifyTrainer(token).ConfigureAwait(false);
            await InitializeHardware(settings, token).ConfigureAwait(false);

            try
            {
                Log($"Starting main {GetType().Name} loop.");
                Config.IterateNextRoutine();

                // Clear out any residual stick weirdness.
                await ResetStick(token).ConfigureAwait(false);
                await EncounterLoop(sav, token).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log(e.Message);
            }

            Log($"Ending {GetType().Name} loop.");
            await HardStop().ConfigureAwait(false);
        }

        public override async Task HardStop()
        {
            await ResetStick(CancellationToken.None).ConfigureAwait(false);
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract Task EncounterLoop(SAV8LA sav, CancellationToken token);

        public void Acknowledge() => throw new NotImplementedException();

        protected async Task ResetStick(CancellationToken token)
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false); // reset
        }
    }
}
