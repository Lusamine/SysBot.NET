using PKHeX.Core;
using System.Threading;
using System.Threading.Tasks;
using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using static SysBot.Pokemon.PokeDataOffsetsFRLG;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotPickupFRLG(PokeBotState cfg, PokeTradeHub<PK3> hub) : EncounterBotFRLG(cfg, hub)
    {
        // Cached offsets that stay the same per session.
        private uint EncounterOffset;
        private bool Horizontal;
        private uint PartyOffset;
        private int FirstPickupIndex = -1; // Index of first party Pokémon that has Pickup
        private int LastPickupIndex; // Index of last party Pokémon that has Pickup
        private int CurrentPP; // Remaining number of times we can start an encounter
        private int MaxPP; // Number of PP after healing
        private readonly int[] Items = [0, 0, 0, 0, 0, 0]; // List of party items.
        private readonly string[] ItemStrings = GameInfo.GetStrings("en").GetItemStrings(EntityContext.Gen3);


        protected override async Task EncounterLoop(SAV3FRLG sav, CancellationToken token)
        {
            await InitializeSessionValues(sav, token).ConfigureAwait(false);

            while (!token.IsCancellationRequested)
            {
                // If the attacking move runs out of PP, heal the party.
                // This will also correct the value of Horizontal after we walk off the array.
                if (CurrentPP == 0)
                    await HealParty(token).ConfigureAwait(false);

                Log("Looking for a Pokémon...");
                var tries = 0;

                PK3? pknew;

                do
                {
                    await WiggleInPlace(token).ConfigureAwait(false);
                    pknew = await ReadUntilPresent(EncounterOffset, 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                    if (++tries > 1000)
                        break;
                } while (pknew is null || pknew.Species == 0);

                // Check for a match, even though our goal is to farm items with Pickup
                if (pknew is not null && await HandleEncounter(pknew, token).ConfigureAwait(false))
                    return;

                Log("No match, clearing the encounter...");
                // Attack with the first move until battle is over.
                while (await IsInBattle(token).ConfigureAwait(false))
                    await Click(A, 0_200, token).ConfigureAwait(false);
                await Task.Delay(0_800, token).ConfigureAwait(false);

                // If we have at least 2 items among our Pickup members, collect them.
                if (await CountPartyItems(token).ConfigureAwait(false) >= 2)
                {
                    Log("Collecting Pickup items...");
                    await Task.Delay(1_000, token).ConfigureAwait(false);
                    await CollectPartyItems(token).ConfigureAwait(false);
                }

                Horizontal = !Horizontal;
                CurrentPP--;
            }
        }

        // These don't change per session, and we access them frequently, so set these each time we start.
        private async Task InitializeSessionValues(SAV3FRLG sav, CancellationToken token)
        {
            Log("Initializing session constants...");
            EncounterOffset = LanguageVersionOffsetsFRLG.GetWildPokemonOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
            Horizontal = true; // Always assume the user starts the bot facing up/down, and our first wiggles are left/right.

            await CheckPartySetup(sav, token).ConfigureAwait(false);
        }

        private async Task CheckPartySetup(SAV3FRLG sav, CancellationToken token)
        {
            // Set the starting party offset to check Pickup at.
            PartyOffset = LanguageVersionOffsetsFRLG.GetPartyStartOffsetFromLanguageAndVersion((LanguageID)sav.Language, sav.Version);
            PK3? pknew;
            for (var i = 0; i < 6; i++)
            {
                pknew = await ReadUntilPresent(PartyOffset + ((ulong)i * 0x64), 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pknew is null || pknew.Species == 0)
                    break; // No more Pokémon in the party, so we can stop checking.

                if (i == 0) // This is the first Pokémon so we'll record the PP of its first move.
                {
                    CurrentPP = pknew.Move1_PP;
                    pknew.HealPP();
                    MaxPP = pknew.Move1_PP;
                    Log($"Detected first Pokémon with {CurrentPP}/{MaxPP} PP.");
                    if (CurrentPP == 0)
                        await HealParty(token).ConfigureAwait(false);
                }

                if (pknew.Ability == (int)Ability.Pickup)
                {
                    if (FirstPickupIndex == -1) // We haven't set the location of the first Pickup Pokémon yet.
                        FirstPickupIndex = i;
                    LastPickupIndex = i; // Update the index of the last Pickup Pokémon we've found.
                }
            }

            if (FirstPickupIndex == -1) // None of the party members had Pickup.
                throw new System.Exception("No Pokémon with Pickup detected in the party. Please make sure at least one party member has the Pickup ability.");
            Log($"Detected first Pickup Pokémon in slot {FirstPickupIndex + 1}.");
            Log($"Detected last Pickup Pokémon in slot {LastPickupIndex + 1}.");
        }

        private async Task<int> CountPartyItems(CancellationToken token)
        {
            PK3? pknew;
            for (var i = FirstPickupIndex; i <= LastPickupIndex; i++)
            {
                pknew = await ReadUntilPresent(PartyOffset + ((ulong)i * 0x64), 0_050, 0_050, BoxFormatSlotSize, token).ConfigureAwait(false);
                if (pknew is null || pknew.Species == 0)
                    break; // Somehow we didn't find a Pokémon.

                if (pknew.Ability == (int)Ability.Pickup)
                {
                    if (pknew.HeldItem != 0)
                        Items[i] = pknew.HeldItem;
                }
            }

            // Count true values in the item array and return it.
            return System.Array.FindAll(Items, item => item != 0).Length;
        }

        private async Task CollectPartyItems(CancellationToken token)
        {
            // Open the Menu. Assumes we were hovered over Pokémon.
            await Click(X, 0_500, token).ConfigureAwait(false);
            await Click(A, 1_800, token).ConfigureAwait(false);
            // We should be hovered over the first party member.

            if (Items[0] != 0) // First party member is holding an item.
                await TakeItem(token).ConfigureAwait(false);

            // Selects the 2nd member of the party.
            await Click(DRIGHT, 0_200, token).ConfigureAwait(false);

            for (var i = 1; i <= LastPickupIndex;)
            {
                var next = FindNextItem(i);
                if (next == -1) // No more items.
                    break;

                // If next item was found, then click DDOWN until we get to it.
                var item = Items[next];
                Log($"Next item is {ItemStrings[item]} in slot {next + 1}.");
                for (; i < next; i++)
                    await Click(DDOWN, 0_200, token).ConfigureAwait(false);

                await TakeItem(token).ConfigureAwait(false);
                Items[i] = 0;
            }
            await Click(B, 1_500, token).ConfigureAwait(false);
            await Click(B, 0_500, token).ConfigureAwait(false);
        }

        private int FindNextItem(int current_index)
        {
            for (var i = current_index; i <= LastPickupIndex; i++)
            {
                if (Items[i] != 0)
                    return i;
            }
            return -1;
        }

        private async Task TakeItem(CancellationToken token)
        {
            await Click(A, 0_200, token).ConfigureAwait(false);
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(DUP, 0_200, token).ConfigureAwait(false);
            await Click(A, 0_200, token).ConfigureAwait(false);
            await Click(DDOWN, 0_200, token).ConfigureAwait(false);
            await Click(A, 0_800, token).ConfigureAwait(false);
            await Click(A, 0_800, token).ConfigureAwait(false);
        }

        private async Task WiggleInPlace(CancellationToken token)
        {
            while (!await IsInBattle(token).ConfigureAwait(false))
            {
                if (Horizontal)
                {
                    for (var i = 0; i < 4; i++)
                    {
                        await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
                        await Task.Delay(0_100, token).ConfigureAwait(false);
                        await Click(DLEFT, 0_050, token).ConfigureAwait(false);
                        await Task.Delay(0_100, token).ConfigureAwait(false);
                    }
                }
                else
                {
                    for (var i = 0; i < 4; i++)
                    {
                        await Click(DUP, 0_050, token).ConfigureAwait(false);
                        await Task.Delay(0_100, token).ConfigureAwait(false);
                        await Click(DDOWN, 0_050, token).ConfigureAwait(false);
                        await Task.Delay(0_100, token).ConfigureAwait(false);
                    }
                }
            }
            await Task.Delay(1_000, token).ConfigureAwait(false); // Extra wait so the encounter is properly loaded.
        }

        // Eventually replace this with a check for whether the battle menu has loaded.
        private async Task EscapeBattle(CancellationToken token)
        {
            for (var i = 0; i < 12; i++)
                await Click(B, 0_500, token).ConfigureAwait(false);

            Log("Initiating run routine.");
            while (await IsInBattle(token).ConfigureAwait(false))
            {
                await Click(DRIGHT, 0_200, token).ConfigureAwait(false);
                await Click(DDOWN, 0_200, token).ConfigureAwait(false);
                await Click(A, 0_500, token).ConfigureAwait(false);
                await Click(B, 0_200, token).ConfigureAwait(false);
                await Click(B, 0_200, token).ConfigureAwait(false);
                await Click(B, 0_200, token).ConfigureAwait(false);
                await Click(B, 0_200, token).ConfigureAwait(false);
            }
            await Task.Delay(0_500, token).ConfigureAwait(false);
        }

        private async Task HealParty(CancellationToken token)
        {
            Log("Healing the party.");
            await SetStick(LEFT, -30000, 0, 0_300, token).ConfigureAwait(false); // Walk left onto the healing array.
            await SetStick(LEFT, 0, 0, 0_150, token).ConfigureAwait(false); // reset

            if (await IsInBattle(token).ConfigureAwait(false)) // Accidentally got attacked.
            {
                await EscapeBattle(token).ConfigureAwait(false);
                await SetStick(LEFT, -30000, 0, 0_300, token).ConfigureAwait(false); // Walk left onto the healing array.
                await SetStick(LEFT, 0, 0, 0_150, token).ConfigureAwait(false); // reset
            }

            // Clear the healing messages.
            for (var i = 0; i < 12; i++)
                await Click(A, 0_200, token).ConfigureAwait(false);
            await SetStick(LEFT, 30000, 0, 0_300, token).ConfigureAwait(false); // Walk right off the healing array.
            await SetStick(LEFT, 0, 0, 0_150, token).ConfigureAwait(false); // reset
            Horizontal = false; // Character will be facing right so we want to start wiggling up/down.
            CurrentPP = MaxPP; // Reset our PP counter.
        }
    }
}
