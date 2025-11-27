using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public abstract class EncounterBotLZA : PokeRoutineExecutor9LZA, IEncounterBot
    {
        protected readonly PokeTradeHub<PA9> Hub;
        private readonly IDumper DumpSetting;
        private readonly EncounterSettingsLZA Settings;
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        public ICountSettings Counts => Settings;

        protected EncounterBotLZA(PokeBotState cfg, PokeTradeHub<PA9> hub) : base(cfg)
        {
            Hub = hub;
            DumpSetting = Hub.Config.Folder;
            Settings = Hub.Config.EncounterLZA;
            StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        }

        protected int encounterCount;

        public override async Task MainLoop(CancellationToken token)
        {
            var settings = Hub.Config.EncounterLZA;
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

        protected abstract Task EncounterLoop(SAV9ZA sav, CancellationToken token);

        // return true if breaking loop
        protected async Task<bool> HandleEncounter(PA9 pk, CancellationToken token)
        {
            encounterCount++;
            var print = StopConditionSettings.GetPrintName(pk);
            Log($"Encounter: {encounterCount}{Environment.NewLine}{print}{Environment.NewLine}");

            var folder = IncrementAndGetDumpFolder(pk);
            if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                DumpPokemon(DumpSetting.DumpFolder, folder, pk);

            if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                return false;

            if (Hub.Config.StopConditions.CaptureVideoClip)
            {
                await Task.Delay(Hub.Config.StopConditions.ExtraTimeWaitCaptureVideo, token).ConfigureAwait(false);
                await PressAndHold(CAPTURE, 2_000, 0, token).ConfigureAwait(false);
            }

            var mode = Settings.ContinueAfterMatch;
            var msg = $"Result found!\n{print}\n" + GetModeMessage(mode);

            if (!string.IsNullOrWhiteSpace(Hub.Config.StopConditions.MatchFoundEchoMention))
                msg = $"{Hub.Config.StopConditions.MatchFoundEchoMention} {msg}";
            EchoUtil.Echo(msg);

            if (mode == ContinueAfterMatch.StopExit)
                return true;
            if (mode == ContinueAfterMatch.Continue)
                return false;

            IsWaiting = true;
            while (IsWaiting)
                await Task.Delay(1_000, token).ConfigureAwait(false);
            return false;
        }

        private static string GetModeMessage(ContinueAfterMatch mode) => mode switch
        {
            ContinueAfterMatch.Continue => "Continuing...",
            ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
            ContinueAfterMatch.StopExit => "Stopping routine execution; restart the bot to search again.",
            _ => throw new ArgumentOutOfRangeException(nameof(mode), mode, "Match result type was invalid."),
        };

        private string IncrementAndGetDumpFolder(PA9 pk)
        {
            if (pk.Species is (int)Species.Tyrunt or (int)Species.Amaura)
            {
                Settings.AddCompletedFossils();
                return "fossil";
            }

            Settings.AddCompletedEncounters();
            return "encounters";
        }

        private bool IsWaiting;
        public void Acknowledge() => throw new NotImplementedException();

        protected async Task ResetStick(CancellationToken token)
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false); // reset
        }
    }
}
