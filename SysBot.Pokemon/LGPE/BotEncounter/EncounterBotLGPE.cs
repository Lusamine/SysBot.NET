using PKHeX.Core;
using SysBot.Base;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;

namespace SysBot.Pokemon
{
    public abstract class EncounterBotLGPE : PokeRoutineExecutor7LGPE, IEncounterBot
    {
        protected readonly PokeTradeHub<PB7> Hub;
        private readonly IDumper DumpSetting;
        private readonly EncounterLGPESettings Settings;
        private readonly int[] DesiredMinIVs;
        private readonly int[] DesiredMaxIVs;
        public ICountSettings Counts => Settings;

        protected EncounterBotLGPE(PokeBotState cfg, PokeTradeHub<PB7> hub) : base(cfg)
        {
            Hub = hub;
            Settings = Hub.Config.EncounterLGPE;
            DumpSetting = Hub.Config.Folder;
            StopConditionSettings.InitializeTargetIVs(Hub.Config, out DesiredMinIVs, out DesiredMaxIVs);
        }

        private int encounterCount;

        public override async Task MainLoop(CancellationToken token)
        {
            var settings = Hub.Config.EncounterRNGBS;
            Log("Identifying trainer data of the host console.");
            var sav = await IdentifyTrainer(token).ConfigureAwait(false);
            await InitializeHardware(settings, token).ConfigureAwait(false);

            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.controllerType, (int)HidDeviceType.HidDeviceType_JoyRight1, UseCRLF);
            await Connection.SendAsync(cmd, token).ConfigureAwait(false);

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
            var cmd = SwitchCommand.Configure(SwitchConfigureParameter.controllerType, (int)HidDeviceType.HidDeviceType_FullKey3, UseCRLF);
            await Connection.SendAsync(cmd, CancellationToken.None).ConfigureAwait(false);
            await CleanExit(CancellationToken.None).ConfigureAwait(false);
        }

        protected abstract Task EncounterLoop(SAV7b sav, CancellationToken token);

        private bool IsWaiting;
        public async void Acknowledge()
        {
            // Resume from HOME menu.
            await Click(HOME, 1_600, CancellationToken.None).ConfigureAwait(false);
            IsWaiting = false;
        }

        protected async Task ResetStick(CancellationToken token)
        {
            // If aborting the sequence, we might have the stick set at some position. Clear it just in case.
            await SetStick(RIGHT, 0, 0, 0_500, token).ConfigureAwait(false); // reset
        }

        public async Task<bool> HandleEncounter(PB7 pk, CancellationToken token)
        {
            // Reset stick while we wait for the encounter to load.
            await ResetStick(token).ConfigureAwait(false);

            encounterCount++;
            var print = StopConditionSettings.GetPrintName(pk);
            Log($"Encounter: {encounterCount}{Environment.NewLine}{print}{Environment.NewLine}");
            Settings.AddCompletedResets();

            if (DumpSetting.Dump && !string.IsNullOrEmpty(DumpSetting.DumpFolder))
                DumpPokemon(DumpSetting.DumpFolder, "lgpe", pk);

            if (!StopConditionSettings.EncounterFound(pk, DesiredMinIVs, DesiredMaxIVs, Hub.Config.StopConditions, null))
                return false;

            var mode = Settings.ContinueAfterMatch;
            var msg = $"Result found!\n{print}\n" + mode switch
            {
                ContinueAfterMatch.Continue             => "Continuing...",
                ContinueAfterMatch.PauseWaitAcknowledge => "Waiting for instructions to continue.",
                ContinueAfterMatch.StopExit             => "Stopping routine execution; restart the bot to search again.",
                _ => throw new ArgumentOutOfRangeException(),
            };

            if (mode is not ContinueAfterMatch.Continue)
            {
                Log("Result found! Stopping routine execution; restart the bot(s) to search again.");
                // If we minimize the game right away before the timer starts, it won't increment in the background.
                await MinimizeGame(token).ConfigureAwait(false);
            }

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

        // Prepares the information to check for a radar encounter.
        // Setting a matchspecies only returns results for that species.
        public async Task<(bool, int)> CheckRadarEncounter(CancellationToken token)
        {
            (int radar_species, int radar_form) = await GetLastSpawnedSpecies(token).ConfigureAwait(false);
            // Realistically, we should not see any Meltan or even some of the mons within this range.
            if (radar_species < 1 || radar_species > 151)
                return (false, -1);

            // Zero it out so we can see consecutive spawns of the same species.
            await ClearLastSpawnedSpecies(token).ConfigureAwait(false);

            var flags = await GetLastSpawnedFlags(token).ConfigureAwait(false);

            return await HandleRadarEncounter(radar_species, radar_form, flags, token).ConfigureAwait(false);
        }

        public async Task<(bool, int)> HandleRadarEncounter(int species, int form, uint flags, CancellationToken token)
        {
            encounterCount++;
            Settings.AddCompletedRadar();

            if (species is (144 or 145 or 146))
                Settings.AddCompletedBirds();

            bool shiny = ((flags >> 1) & 1) == 1;
            string shinystring = shiny ? " - shiny!!!" : "";

            uint gender = flags & 1;
            var gender_ratio = PersonalTable.LG[species].Gender;
            string gender_string = gender_ratio switch
            {
                PersonalInfo.RatioMagicGenderless => "",
                PersonalInfo.RatioMagicFemale => " (F)",
                PersonalInfo.RatioMagicMale => " (M)",
                _ => gender == 0 ? " (M)" : " (F)",
            };

            var formstring = form == 0 ? "" : $"-{form}";
            Log($"Encounter detected: {encounterCount} - {(Species)species}{formstring}{gender_string}{shinystring}");

            if (RadarMatch(species, shiny, Hub.Config.StopConditions))
            {
                Log("Result found! Stopping routine execution; restart the bot(s) to search again.");
                if (await IsOnOverworldBattle(token).ConfigureAwait(false))
                    await MinimizeGame(token).ConfigureAwait(false);
                return (true, species);
            }
            return (false, species);
        }

        public static bool RadarMatch(int species, bool shiny, StopConditionSettings settings)
        {
            var match_target_species = settings.StopOnSpecies == Species.None || (int)settings.StopOnSpecies == species;
            var match_all_birds = settings.StopOnAllBirdsLGPE && species is (144 or 145 or 146);

            // Only reject species if it doesn't match the specified species and isn't a bird.
            if (!match_target_species && !match_all_birds)
                return false;

            if (settings.ShinyTarget != TargetShinyType.DisableOption)
            {
                if (settings.ShinyTarget == TargetShinyType.NonShiny && shiny)
                    return false;
                if (settings.ShinyTarget == TargetShinyType.AnyShiny && !shiny)
                    return false;
            }
            return true;
        }

        public async Task FleeToOverworld(CancellationToken token)
        {
            while (!await IsOnOverworldBattle(token).ConfigureAwait(false) && !await IsOnFleeMenu(token).ConfigureAwait(false))
                await Click(B, 0_500, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);
        }
    }
}
