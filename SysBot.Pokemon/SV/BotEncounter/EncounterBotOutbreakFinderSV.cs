using static SysBot.Base.SwitchButton;
using static SysBot.Base.SwitchStick;
using System.Threading;
using System.Threading.Tasks;
using PKHeX.Core;
using System;

namespace SysBot.Pokemon
{
    public sealed class EncounterBotOutbreakFinderSV(PokeBotState cfg, PokeTradeHub<PK9> hub) : EncounterBotSV(cfg, hub)
    {
        // Paldea
        private const uint EncountOutbreakSave_enablecount = 0x6C375C8A;

        private readonly uint[] EncountOutbreakSave_centerPos           = [0x2ED42F4D, 0x2ED5F198, 0x2ECE09D3, 0x2ED04676, 0x2EC78531, 0x2ECB673C, 0x2EC1CC77, 0x2EC5BC1A];
        private readonly uint[] EncountOutbreakSave_dummyPos            = [0x4A13BE7C, 0x4A118F71, 0x4A0E135A, 0x4A0BD6B7, 0x4A1FFBD8, 0x4A1C868D, 0x4A1A50B6, 0x4A166113];
        private readonly uint[] EncountOutbreakSave_monsno              = [0x76A2F996, 0x76A0BCF3, 0x76A97E38, 0x76A6E26D, 0x76986F3A, 0x76947F97, 0x769D40DC, 0x769B11D1];
        private readonly uint[] EncountOutbreakSave_formno              = [0x29B4615D, 0x29B84368, 0x29AF8223, 0x29B22B86, 0x29A9D701, 0x29AB994C, 0x29A344C7, 0x29A5EE2A];
        private readonly uint[] EncountOutbreakSave_isFind              = [0x7E203623, 0x7E22DF86, 0x7E25155D, 0x7E28F768, 0x7E13F8C7, 0x7E16A22A, 0x7E1A8B01, 0x7E1C4D4C];
        private readonly uint[] EncountOutbreakSave_subjugationCount    = [0x4B16FBC2, 0x4B14BF1F, 0x4B1CA6E4, 0x4B1A77D9, 0x4B23391E, 0x4B208FBB, 0x4B28E440, 0x4B256EF5];
        private readonly uint[] EncountOutbreakSave_subjugationLimit    = [0xB7DC495A, 0xB7DA0CB7, 0xB7E1F47C, 0xB7DFC571, 0xB7E886B6, 0xB7E49713, 0xB7EE31D8, 0xB7EABC8D];

        // Kitakami
        private const uint EncountOutbreakSave_f1_enablecount = 0xBD7C2A04;

        private readonly uint[] EncountOutbreakSave_f1_centerPos        = [0x411A0C07, 0x411CB56A, 0x411EEB41, 0x4122608C];
        private readonly uint[] EncountOutbreakSave_f1_dummyPos         = [0x632EFBFE, 0x632D2C1B, 0x633580A0, 0x6332E4D5];
        private readonly uint[] EncountOutbreakSave_f1_monsno           = [0x37E55F64, 0x37E33059, 0x37DFB442, 0x37DD779F];
        private readonly uint[] EncountOutbreakSave_f1_formno           = [0x69A930AB, 0x69AD204E, 0x69AEE965, 0x69B2CB70];
        private readonly uint[] EncountOutbreakSave_f1_isFind           = [0x7B688081, 0x7B6A42CC, 0x7B61EE47, 0x7B6497AA];
        private readonly uint[] EncountOutbreakSave_f1_subjugationCount = [0xB29D7978, 0xB29ADDAD, 0xB298A7D6, 0xB294B833];
        private readonly uint[] EncountOutbreakSave_f1_subjugationLimit = [0x9E16873C, 0x9E12A531, 0x9E10DC1A, 0x9E0CEC77];

        // Blueberry
        private const uint EncountOutbreakSave_f2_enablecount = 0x19A98811;

        private readonly uint[] EncountOutbreakSave_f2_centerPos        = [0xCE463C0C, 0xCE42C6C1, 0xCE4090EA, 0xCE3DE787, 0xCE513328];
        private readonly uint[] EncountOutbreakSave_f2_dummyPos         = [0x0B0C71CB, 0x0B10616E, 0x0B130405, 0x0B153310, 0x0B01E76F];
        private readonly uint[] EncountOutbreakSave_f2_monsno           = [0xB8E99C8D, 0xB8ED11D8, 0xB8E37713, 0xB8E766B6, 0xB8DEA571];
        private readonly uint[] EncountOutbreakSave_f2_formno           = [0xEFA6983A, 0xEFA2A897, 0xEFAB69DC, 0xEFA93AD1, 0xEFB12296];
        private readonly uint[] EncountOutbreakSave_f2_isFind           = [0x32074910, 0x32051A05, 0x3202776E, 0x31FE87CB, 0x31FCBEB4];
        private readonly uint[] EncountOutbreakSave_f2_subjugationCount = [0x4EF9BC25, 0x4EFBEB30, 0x4EF4036B, 0x4EF6400E, 0x4EED7EC9];
        private readonly uint[] EncountOutbreakSave_f2_subjugationLimit = [0x4385E0AD, 0x43887C78, 0x437FBB33, 0x4383AAD6, 0x437A1011];

        // Offsets to cache to reduce reads.
        private ulong BlockKeyOffset;
        private ulong ActiveOutbreaksOffset;
        private readonly ulong[] monsno = new ulong[8];
        private readonly ulong[] formno = new ulong[8];
        private readonly ulong[] centerPos = new ulong[8];

        private ulong ActiveOutbreaksOffset_f1;
        private readonly ulong[] monsno_f1 = new ulong[8];
        private readonly ulong[] formno_f1 = new ulong[8];
        private readonly ulong[] centerPos_f1 = new ulong[8];

        private ulong ActiveOutbreaksOffset_f2;
        private readonly ulong[] monsno_f2 = new ulong[8];
        private readonly ulong[] formno_f2 = new ulong[8];
        private readonly ulong[] centerPos_f2 = new ulong[8];

        // Counter to group outbreaks together.
        private int outbreak_counter;
        private readonly float[] prev_coords = new float[3];

        protected override async Task EncounterLoop(SAV9SV sav, CancellationToken token)
        {
            await InitializeSessionOffsets(token).ConfigureAwait(false);

            var target_species = (int)Hub.Config.StopConditions.StopOnSpecies;
            var target_form = Hub.Config.StopConditions.StopOnForm;

            // Open the X menu since we must save every time.
            await Click(X, 1_000, token).ConfigureAwait(false);
            const bool mainpaldea = true;
            const bool kitakami = true;
            const bool blueberry = true;

            while (!token.IsCancellationRequested)
            {
                outbreak_counter++;

                //Log("Rolling the date...");
                await RollDate(token).ConfigureAwait(false);

                //Log("Saving the game...");
                await Click(R, 0_700, token).ConfigureAwait(false);
                await Click(A, 3_300, token).ConfigureAwait(false);
                await Click(A, 1_300, token).ConfigureAwait(false);

                // Main Paldea
                if (mainpaldea)
                {
                    //Log("Checking all the outbreaks...");
                    var active_cnt = await GetOutbreakBlockValueByte(ActiveOutbreaksOffset, EncountOutbreakSave_enablecount, token).ConfigureAwait(false);
                    if (active_cnt == 0)
                    {
                        Log("No outbreaks found.");
                        continue;
                    }

                    for (int i = 0; i < active_cnt; i++)
                    {
                        var species = await GetOutbreakBlockValueUInt32(monsno[i], EncountOutbreakSave_monsno[i], token).ConfigureAwait(false);
                        var form = await GetOutbreakBlockValueByte(formno[i], EncountOutbreakSave_formno[i], token).ConfigureAwait(false);
                        var center_pos = await GetOutbreakBlockValueArray(centerPos[i], EncountOutbreakSave_centerPos[i], token).ConfigureAwait(false);

                        var species_national = SpeciesConverter.GetNational9((ushort)species);
                        var center_coords = FetchCoordinates(center_pos);

                        if (i == 0)
                        {
                            if (prev_coords[0] == center_coords[0] && prev_coords[1] == center_coords[1] && prev_coords[2] == center_coords[2])
                            {
                                Log("Coordinates did not change, skipping...");
                                for (int j = 0; j < 10; j++)
                                    await Click(B, 0_800, token).ConfigureAwait(false);
                                await Click(X, 1_000, token).ConfigureAwait(false);
                                break;
                            }
                            Log($"P | {outbreak_counter} | {active_cnt} active outbreaks");
                            // Store them for the next pass.
                            (prev_coords[0], prev_coords[1], prev_coords[2]) = (center_coords[0], center_coords[1], center_coords[2]);
                        }

                        //var dummy_coords  = FetchCoordinates(dummy_pos);
                        var output = $"P | {outbreak_counter} | {species} | {GameInfo.GetStrings(1).Species[species_national]} | {form} | {center_coords[0]}, {center_coords[1]}, {center_coords[2]}";
                        Log(output);

                        if (species == target_species && (target_form == null || form == target_form))
                        {
                            var form_string = form == 0 ? "" : $"-{form}";
                            Log($"Found a {(Species)species}{form_string} outbreak!");
                            return;
                        }
                    }
                }

                // Kitakami
                if (kitakami)
                {
                    //Log("Checking all the outbreaks...");
                    var active_cnt = await GetOutbreakBlockValueByte(ActiveOutbreaksOffset_f1, EncountOutbreakSave_f1_enablecount, token).ConfigureAwait(false);
                    if (active_cnt == 0)
                    {
                        Log("No outbreaks found.");
                        continue;
                    }

                    for (int i = 0; i < active_cnt; i++)
                    {
                        var species = await GetOutbreakBlockValueUInt32(monsno_f1[i], EncountOutbreakSave_f1_monsno[i], token).ConfigureAwait(false);
                        var form = await GetOutbreakBlockValueByte(formno_f1[i], EncountOutbreakSave_f1_formno[i], token).ConfigureAwait(false);
                        var center_pos = await GetOutbreakBlockValueArray(centerPos_f1[i], EncountOutbreakSave_f1_centerPos[i], token).ConfigureAwait(false);

                        var species_national = SpeciesConverter.GetNational9((ushort)species);
                        var center_coords = FetchCoordinates(center_pos);

                        if (i == 0)
                            Log($"K | {outbreak_counter} | {active_cnt} active outbreaks");

                        //var dummy_coords  = FetchCoordinates(dummy_pos);
                        var output = $"K | {outbreak_counter} | {species} | {GameInfo.GetStrings(1).Species[species_national]} | {form} | {center_coords[0]}, {center_coords[1]}, {center_coords[2]}";
                        Log(output);

                        if (species == target_species && (target_form == null || form == target_form))
                        {
                            var form_string = form == 0 ? "" : $"-{form}";
                            Log($"Found a {(Species)species}{form_string} outbreak!");
                            return;
                        }
                    }
                }

                // Blueberry
                if (blueberry)
                {
                    //Log("Checking all the outbreaks...");
                    var active_cnt = await GetOutbreakBlockValueByte(ActiveOutbreaksOffset_f2, EncountOutbreakSave_f2_enablecount, token).ConfigureAwait(false);
                    if (active_cnt == 0)
                    {
                        Log("No outbreaks found.");
                        continue;
                    }

                    for (int i = 0; i < active_cnt; i++)
                    {
                        var species = await GetOutbreakBlockValueUInt32(monsno_f2[i], EncountOutbreakSave_f2_monsno[i], token).ConfigureAwait(false);
                        var form = await GetOutbreakBlockValueByte(formno_f2[i], EncountOutbreakSave_f2_formno[i], token).ConfigureAwait(false);
                        var center_pos = await GetOutbreakBlockValueArray(centerPos_f2[i], EncountOutbreakSave_f2_centerPos[i], token).ConfigureAwait(false);

                        var species_national = SpeciesConverter.GetNational9((ushort)species);
                        var center_coords = FetchCoordinates(center_pos);

                        if (i == 0)
                            Log($"B | {outbreak_counter} | {active_cnt} active outbreaks");

                        //var dummy_coords  = FetchCoordinates(dummy_pos);
                        var output = $"B | {outbreak_counter} | {species} | {GameInfo.GetStrings(1).Species[species_national]} | {form} | {center_coords[0]}, {center_coords[1]}, {center_coords[2]}";
                        Log(output);

                        if (species == target_species && (target_form == null || form == target_form))
                        {
                            var form_string = form == 0 ? "" : $"-{form}";
                            Log($"Found a {(Species)species}{form_string} outbreak!");
                            return;
                        }
                    }
                }
            }

            Log("Done!");
            return;
        }

        private async Task InitializeSessionOffsets(CancellationToken token)
        {
            Log("Caching session offsets...");
            BlockKeyOffset = await SwitchConnection.PointerAll(Offsets.BlockKeyStructPointer, token).ConfigureAwait(false);
            ActiveOutbreaksOffset = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_enablecount, token).ConfigureAwait(false);
            ActiveOutbreaksOffset_f1 = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f1_enablecount, token).ConfigureAwait(false);
            ActiveOutbreaksOffset_f2 = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f2_enablecount, token).ConfigureAwait(false);

            // Paldea
            for (int i = 0; i < 8; i++)
            {
                monsno[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_monsno[i], token).ConfigureAwait(false);
                formno[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_formno[i], token).ConfigureAwait(false);
                centerPos[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_centerPos[i], token).ConfigureAwait(false);
            }

            // Kitakami
            for (int i = 0; i < 4; i++)
            {
                monsno_f1[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f1_monsno[i], token).ConfigureAwait(false);
                formno_f1[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f1_formno[i], token).ConfigureAwait(false);
                centerPos_f1[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f1_centerPos[i], token).ConfigureAwait(false);
            }

            // Blueberry
            for (int i = 0; i < 5; i++)
            {
                monsno_f2[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f2_monsno[i], token).ConfigureAwait(false);
                formno_f2[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f2_formno[i], token).ConfigureAwait(false);
                centerPos_f2[i] = await GetKeyOffset(BlockKeyOffset, EncountOutbreakSave_f2_centerPos[i], token).ConfigureAwait(false);
            }
        }

        // This has a tendency to turn your console language to German, so you may want to set it beforehand to avoid a reboot.
        private async Task RollDate(CancellationToken token)
        {
            await Click(RSTICK, 0_800, token).ConfigureAwait(false);

            // HOME Menu
            await Click(HOME, 0_800, token).ConfigureAwait(false);

            // Navigate to Settings
            await Touch(0_909, 0_550, 0_050, 0, token).ConfigureAwait(false);
            await Click(A, 1_000, token).ConfigureAwait(false);

            // Scroll to bottom
            await PressAndHold(DDOWN, 1700, 0_150, token).ConfigureAwait(false);

            // Navigate to "Date and Time"
            await Click(A, 0_300, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, -30000, 0_830, token).ConfigureAwait(false);
            await SetStick(LEFT, 0, 0, 0_500, token).ConfigureAwait(false);

            await Click(A, 0_100, token).ConfigureAwait(false);
            await Touch(950, 400, 0_100, 0_300, token).ConfigureAwait(false);
            for (int i = 0; i < 6; i++)
                await Click(DRIGHT, 0_050, token).ConfigureAwait(false);
            await Touch(950, 400, 0_100, 0_300, token).ConfigureAwait(false);
            await Click(A, 0_100, token).ConfigureAwait(false);

            await Click(A, 0_100, token).ConfigureAwait(false);

            for (int i = 0; i < 6; i++)
                await Click(DRIGHT, 0_105, token).ConfigureAwait(false);
            await Click(A, 0_500, token).ConfigureAwait(false);

            // Return to game
            await Click(HOME, 0_800, token).ConfigureAwait(false);
            await Click(HOME, 2_500, token).ConfigureAwait(false);
        }

        // EncountOutbreakSave_centerPos and EncountOutbreakSave_dummyPos
        private async Task<byte[]> GetOutbreakBlockValueArray(ulong offset, uint key, CancellationToken token)
        {
            var (address, size) = await ReadKeyData(offset, key, token).ConfigureAwait(false);
            return await GetValueFromBlockKeyOffset(address, size, key, token).ConfigureAwait(false);
        }

        // EncountOutbreakSave_monsno, EncountOutbreakSave_subjugationCount, and EncountOutbreakSave_subjugationLimit
        private async Task<uint> GetOutbreakBlockValueUInt32(ulong offset, uint key, CancellationToken token)
        {
            var (address, size) = await ReadKeyData(offset, key, token).ConfigureAwait(false);
            var data = await GetValueFromBlockKeyOffset(address, size, key, token).ConfigureAwait(false);
            return BitConverter.ToUInt32(data, 0);
        }

        // EncountOutbreakSave_enablecount and EncountOutbreakSave_formno
        private async Task<byte> GetOutbreakBlockValueByte(ulong offset, uint key, CancellationToken token)
        {
            var (address, size) = await ReadKeyData(offset, key, token).ConfigureAwait(false);
            var data = await GetValueFromBlockKeyOffset(address, size, key, token).ConfigureAwait(false);
            return data[0];
        }

        // EncountOutbreakSave_isFind
        private async Task<byte> GetOutbreakBlockValueBool(ulong offset, uint key, CancellationToken token)
        {
            var (address, size) = await ReadKeyData(offset, key, token).ConfigureAwait(false);
            var data = await GetValueFromBlockKeyOffset(address, size, key, token).ConfigureAwait(false);
            return data[0];
        }

        private static float[] FetchCoordinates(byte[] data)
        {
            var coords = new float[3];
            coords[0] = BitConverter.ToSingle(data, 0);
            coords[1] = BitConverter.ToSingle(data, 4);
            coords[2] = BitConverter.ToSingle(data, 8);
            return coords;
        }
    }
}
