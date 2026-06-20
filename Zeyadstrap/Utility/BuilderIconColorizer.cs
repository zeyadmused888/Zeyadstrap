using System.Buffers.Binary;

namespace Zeyadstrap.Utility
{
    public static class BuilderIconColorizer
    {
        public const string RelativeDirectory = @"ExtraContent\LuaPackages\Packages\_Index\BuilderIcons\BuilderIcons";

        private const string FontDirectory = RelativeDirectory + @"\Font";
        private const string RegularFontName = "BuilderIcons-Regular";
        private const string FilledFontName = "BuilderIcons-Filled";

        private static readonly string[] GeneratedFiles =
        {
            Path.Combine(FontDirectory, $"{RegularFontName}.otf"),
            Path.Combine(FontDirectory, $"{FilledFontName}.otf"),
            Path.Combine(RelativeDirectory, "BuilderIcons.json")
        };

        public static IEnumerable<string> GeneratedRelativeFiles => GeneratedFiles;

        public static bool IsValidHexColor(string value)
        {
            return Regex.IsMatch(value, "^#[0-9A-Fa-f]{6}$", RegexOptions.CultureInvariant);
        }

        public static void RemoveModFiles()
        {
            foreach (string relativeFile in GeneratedFiles)
            {
                string path = Path.Combine(Paths.Modifications, relativeFile);

                if (File.Exists(path))
                {
                    Filesystem.AssertReadOnly(path);
                    File.Delete(path);
                }
            }
        }

        public static void GenerateModFiles(string versionDirectory, string hexColor)
        {
            if (!IsValidHexColor(hexColor))
                hexColor = "#FFFFFF";

            var color = ParseHexColor(hexColor);

            string sourceFontDirectory = Path.Combine(versionDirectory, FontDirectory);
            string modFontDirectory = Path.Combine(Paths.Modifications, FontDirectory);

            string regularSource = Path.Combine(sourceFontDirectory, $"{RegularFontName}.ttf");
            string filledSource = Path.Combine(sourceFontDirectory, $"{FilledFontName}.ttf");

            if (!File.Exists(regularSource) || !File.Exists(filledSource))
                throw new FileNotFoundException("BuilderIcons fonts were not found in the Roblox installation.");

            Directory.CreateDirectory(modFontDirectory);

            WriteColorizedFont(regularSource, Path.Combine(modFontDirectory, $"{RegularFontName}.otf"), color);
            WriteColorizedFont(filledSource, Path.Combine(modFontDirectory, $"{FilledFontName}.otf"), color);

            string jsonPath = Path.Combine(Paths.Modifications, RelativeDirectory, "BuilderIcons.json");
            Directory.CreateDirectory(Path.GetDirectoryName(jsonPath)!);
            Filesystem.AssertReadOnly(jsonPath);
            File.WriteAllText(jsonPath, JsonSerializer.Serialize(new FontFamily
            {
                Name = "Builder Icons",
                LoadStrategy = "sameFamilyOnly",
                Faces = new[]
                {
                    new FontFace
                    {
                        Name = "Regular",
                        Weight = 400,
                        Style = "normal",
                        AssetId = "rbxasset://LuaPackages/Packages/_Index/BuilderIcons/BuilderIcons/Font/BuilderIcons-Regular.otf"
                    },
                    new FontFace
                    {
                        Name = "Bold",
                        Weight = 700,
                        Style = "normal",
                        AssetId = "rbxasset://LuaPackages/Packages/_Index/BuilderIcons/BuilderIcons/Font/BuilderIcons-Filled.otf"
                    }
                }
            }, new JsonSerializerOptions { WriteIndented = true }));
        }

        private static (byte R, byte G, byte B) ParseHexColor(string value)
        {
            return (
                Convert.ToByte(value.Substring(1, 2), 16),
                Convert.ToByte(value.Substring(3, 2), 16),
                Convert.ToByte(value.Substring(5, 2), 16)
            );
        }

        private static void WriteColorizedFont(string inputPath, string outputPath, (byte R, byte G, byte B) color)
        {
            byte[] fontData = File.ReadAllBytes(inputPath);
            byte[] output = InjectColorTables(fontData, color);

            Filesystem.AssertReadOnly(outputPath);
            File.WriteAllBytes(outputPath, output);
        }

        private static byte[] InjectColorTables(byte[] fontData, (byte R, byte G, byte B) color)
        {
            ushort numTables = ReadUInt16(fontData, 4);
            uint sfntVersion = ReadUInt32(fontData, 0);

            var tableData = new Dictionary<string, byte[]>();

            for (int i = 0; i < numTables; i++)
            {
                int entryOffset = 12 + i * 16;
                string tag = Encoding.ASCII.GetString(fontData, entryOffset, 4);
                uint offset = ReadUInt32(fontData, entryOffset + 8);
                uint length = ReadUInt32(fontData, entryOffset + 12);

                byte[] data = new byte[(int)length];
                Buffer.BlockCopy(fontData, (int)offset, data, 0, (int)length);
                tableData[tag] = data;
            }

            ushort glyphCount = GetGlyphCount(tableData);
            tableData["COLR"] = CreateCOLRTable(glyphCount);
            tableData["CPAL"] = CreateCPALTable(color);

            var tags = tableData.Keys.OrderBy(x => x, StringComparer.Ordinal).ToList();
            ushort newNumTables = (ushort)tags.Count;
            ushort searchRange = (ushort)(Math.Pow(2, Math.Floor(Math.Log2(newNumTables))) * 16);
            ushort entrySelector = (ushort)Math.Floor(Math.Log2(newNumTables));
            ushort rangeShift = (ushort)(newNumTables * 16 - searchRange);

            int currentOffset = Align4(12 + newNumTables * 16);
            var tableOffsets = new Dictionary<string, int>();

            foreach (string tag in tags)
            {
                tableOffsets[tag] = currentOffset;
                currentOffset += Align4(tableData[tag].Length);
            }

            byte[] output = new byte[currentOffset];

            WriteUInt32(output, 0, sfntVersion);
            WriteUInt16(output, 4, newNumTables);
            WriteUInt16(output, 6, searchRange);
            WriteUInt16(output, 8, entrySelector);
            WriteUInt16(output, 10, rangeShift);

            int headTableOffset = 0;
            int headTableIndex = -1;

            for (int i = 0; i < tags.Count; i++)
            {
                string tag = tags[i];
                byte[] data = tableData[tag];

                if (tag == "head" && data.Length >= 12)
                {
                    data = data.ToArray();
                    WriteUInt32(data, 8, 0);
                    tableData[tag] = data;
                    headTableOffset = tableOffsets[tag];
                    headTableIndex = i;
                }

                int entryOffset = 12 + i * 16;
                Encoding.ASCII.GetBytes(tag, output.AsSpan(entryOffset, 4));
                WriteUInt32(output, entryOffset + 4, CalculateTableChecksum(data));
                WriteUInt32(output, entryOffset + 8, (uint)tableOffsets[tag]);
                WriteUInt32(output, entryOffset + 12, (uint)data.Length);

                Buffer.BlockCopy(data, 0, output, tableOffsets[tag], data.Length);
            }

            if (headTableIndex >= 0)
            {
                uint checksumAdjustment = 0xB1B0AFBA - CalculateWholeFileChecksum(output);
                WriteUInt32(output, headTableOffset + 8, checksumAdjustment);

                byte[] headData = new byte[tableData["head"].Length];
                Buffer.BlockCopy(output, headTableOffset, headData, 0, headData.Length);
                WriteUInt32(output, 12 + headTableIndex * 16 + 4, CalculateTableChecksum(headData));
            }

            return output;
        }

        private static ushort GetGlyphCount(Dictionary<string, byte[]> tableData)
        {
            if (tableData.TryGetValue("maxp", out byte[]? maxpData) && maxpData.Length >= 6)
                return ReadUInt16(maxpData, 4);

            return 256;
        }

        private static byte[] CreateCPALTable((byte R, byte G, byte B) color)
        {
            byte[] buffer = new byte[18];

            WriteUInt16(buffer, 0, 0);
            WriteUInt16(buffer, 2, 1);
            WriteUInt16(buffer, 4, 1);
            WriteUInt16(buffer, 6, 1);
            WriteUInt32(buffer, 8, 14);
            WriteUInt16(buffer, 12, 0);
            buffer[14] = color.B;
            buffer[15] = color.G;
            buffer[16] = color.R;
            buffer[17] = 255;

            return buffer;
        }

        private static byte[] CreateCOLRTable(ushort glyphCount)
        {
            const int headerSize = 14;
            const int baseGlyphRecordSize = 6;
            const int layerRecordSize = 4;

            int offsetLayerRecord = headerSize + glyphCount * baseGlyphRecordSize;
            byte[] buffer = new byte[offsetLayerRecord + glyphCount * layerRecordSize];

            WriteUInt16(buffer, 0, 0);
            WriteUInt16(buffer, 2, glyphCount);
            WriteUInt32(buffer, 4, headerSize);
            WriteUInt32(buffer, 8, (uint)offsetLayerRecord);
            WriteUInt16(buffer, 12, glyphCount);

            for (ushort i = 0; i < glyphCount; i++)
            {
                int offset = headerSize + i * baseGlyphRecordSize;
                WriteUInt16(buffer, offset, i);
                WriteUInt16(buffer, offset + 2, i);
                WriteUInt16(buffer, offset + 4, 1);
            }

            for (ushort i = 0; i < glyphCount; i++)
            {
                int offset = offsetLayerRecord + i * layerRecordSize;
                WriteUInt16(buffer, offset, i);
                WriteUInt16(buffer, offset + 2, 0);
            }

            return buffer;
        }

        private static uint CalculateTableChecksum(byte[] data)
        {
            int paddedLength = Align4(data.Length);
            byte[] padded = new byte[paddedLength];
            Buffer.BlockCopy(data, 0, padded, 0, data.Length);

            uint sum = 0;

            for (int i = 0; i < paddedLength; i += 4)
                sum += ReadUInt32(padded, i);

            return sum;
        }

        private static uint CalculateWholeFileChecksum(byte[] data)
        {
            return CalculateTableChecksum(data);
        }

        private static int Align4(int value) => (value + 3) & ~3;

        private static ushort ReadUInt16(byte[] data, int offset) => BinaryPrimitives.ReadUInt16BigEndian(data.AsSpan(offset, 2));

        private static uint ReadUInt32(byte[] data, int offset) => BinaryPrimitives.ReadUInt32BigEndian(data.AsSpan(offset, 4));

        private static void WriteUInt16(byte[] data, int offset, ushort value) => BinaryPrimitives.WriteUInt16BigEndian(data.AsSpan(offset, 2), value);

        private static void WriteUInt32(byte[] data, int offset, uint value) => BinaryPrimitives.WriteUInt32BigEndian(data.AsSpan(offset, 4), value);
    }
}