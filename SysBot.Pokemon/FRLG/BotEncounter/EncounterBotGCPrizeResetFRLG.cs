using PKHeX.Core;
using System;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Pokemon.PokeDataOffsetsFRLG;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotGCPrizeResetFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private uint FirstEmptySlotOffset;
        private int PartyCount;
        private int PrizesToBuy;
        private int Displacement;
        private int ExtraTimeReset;
        private int ExtraTimeEncounter;
        private uint OverworldOffset;

        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            await InitializeSessionValues(sav, token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                Log("Looking for a Pokémon...");
                var tries = 0;

                PK3? pknew;

                // Waits a random number of milliseconds to increase the number of possible RNG states.
                if (ExtraTimeEncounter > 0)
                {
                    var extra = Util.Rand.Next(0, ExtraTimeEncounter);
                    await Task.Delay(extra, token).ConfigureAwait(false);
                    Log($"Waiting an extra {extra}ms before purchasing prizes to increase RNG variability.");
                }

                await SelectPrize(token).ConfigureAwait(false);

                for (var purchased = 0; purchased < PrizesToBuy;)
                {
                    do
                    {
                        await Click(A, 0_500, token).ConfigureAwait(false);
                        pknew = await ReadUntilPresent(FirstEmptySlotOffset + ((ulong)purchased * 0x64), 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                        if (++tries > 1000)
                            break;
                    } while (pknew is null || pknew.Species == 0);

                    // Check for a match
                    if (pknew is not null && await HandleEncounter(pknew, token).ConfigureAwait(false))
                        return;

                    if (++purchased == PrizesToBuy)
                        break;

                    for (int i = 0; i < 15; i++)
                        await Click(B, 0_200, token).ConfigureAwait(false);
                }

                Log("No match, resetting the game...");
                await SoftResetGame(OverworldOffset, ExtraTimeReset, token).ConfigureAwait(false);
                await Task.Delay(2_000, token).ConfigureAwait(false);
            }
        }

        // These don't change per session, and we access them frequently, so set these each time we start.
        private async Task InitializeSessionValues(SAV3FRLG sav, CancellationToken token)
        {
            Log("Initializing session constants...");
            OverworldOffset = LanguageVersionOffsetsFRLG.GetOverworldOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);

            // This will let us know how many empty slots we have for gift encounters. Ideally, PartyCount is 1.
            PartyCount = await GetPartyCount(token).ConfigureAwait(false);
            if (PartyCount == 6)
                throw new System.Exception("All 6 party slots are full. Clear out at least one empty slot for gift encounters.");

            // Set the starting party offset to check gifts at.
            FirstEmptySlotOffset = LanguageVersionOffsetsFRLG.GetPartyStartOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
            FirstEmptySlotOffset += (uint)(PartyCount * 0x64); // The next empty slot.

            PrizesToBuy = Math.Min(Hub.Config.EncounterFRLG.GameCornerNumberToPurchase, 6 - PartyCount);
            Displacement = GetPrizeDisplacement(sav.Version);
            if (Displacement == -1)
                throw new Exception("Invalid prize selection. Check the GameCornerPrizeToPurchase before restarting the bot.");
            ExtraTimeEncounter = Hub.Config.EncounterFRLG.RandomTimeBeforeEncounter;
            ExtraTimeReset = Hub.Config.EncounterFRLG.RandomTimeSoftReset;
        }

        private async Task SelectPrize(CancellationToken token)
        {
            await Click(A, 1_000, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Fewest button presses to get to the prize.
            if (Displacement <= 3)
            {
                for (var clicks = 0; clicks < Displacement; clicks++)
                    await Click(DDOWN, 0_200, token).ConfigureAwait(false);
            }
            else if (Displacement == 4)
            {
                await Click(DUP, 0_200, token).ConfigureAwait(false);
                await Click(DUP, 0_200, token).ConfigureAwait(false);
            }
        }

        // FR is in the order Abra, Clefairy, Dratini, Scyther, Porygon. LG is in the order Abra, Clefairy, Pinsir, Dratini, Porygon.
        private int GetPrizeDisplacement(GameVersion version)
        {
            var prize = Hub.Config.EncounterFRLG.GameCornerPrizeToPurchase;

            return prize switch
            {
                GameCornerPrizeFRLG.Abra => 0,
                GameCornerPrizeFRLG.Clefairy => 1,
                GameCornerPrizeFRLG.Dratini => version switch
                {
                    GameVersion.FR => 2,
                    GameVersion.LG => 3,
                    _ => -1
                },
                GameCornerPrizeFRLG.Scyther => version switch
                {
                    GameVersion.FR => 3,
                    GameVersion.LG => throw new Exception("Scyther is not a prize in LG. Check the GameCornerPrizeToPurchase before restarting the bot."),
                    _ => -1
                },
                GameCornerPrizeFRLG.Pinsir => version switch
                {
                    GameVersion.LG => 2,
                    GameVersion.FR => throw new Exception("Pinsir is not a prize in FR. Check the GameCornerPrizeToPurchase before restarting the bot."),
                    _ => -1
                },
                GameCornerPrizeFRLG.Porygon => 4,
                _ => -1,
            };
        }
    }

    public enum GameCornerPrizeFRLG
    {
        Abra,
        Clefairy,
        Dratini,
        Scyther,
        Pinsir,
        Porygon,
    }
}
