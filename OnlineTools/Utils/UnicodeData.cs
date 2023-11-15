using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Unicode;

namespace OnlineTools.Utils
{
    public static class UnicodeData
    {
        private static List<IndexEntry> s_index;

        public static ICollection<CharInfoWrapper> GetData(string search)
        {
            if (search.Length == 0)
                return Array.Empty<CharInfoWrapper>();

            if (search.Length == 1)
                return new[] { new CharInfoWrapper(UnicodeInfo.GetCharInfo(search[0])) };

            if (search.Length == 2 && char.IsHighSurrogate(search[0]) && char.IsLowSurrogate(search[1]))
                return new[] { new CharInfoWrapper(UnicodeInfo.GetCharInfo(char.ConvertToUtf32(search[0], search[1]))) };

            int code;
            if (search.StartsWith("\\u", StringComparison.OrdinalIgnoreCase) || search.StartsWith("U+", StringComparison.OrdinalIgnoreCase) || search.StartsWith("&#", StringComparison.OrdinalIgnoreCase))
            {
                var value = search[2..];
                if (int.TryParse(value, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out code))
                    return new[] { new CharInfoWrapper(UnicodeInfo.GetCharInfo(code)) };
            }

            if (int.TryParse(search, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out code))
                return new[] { new CharInfoWrapper(UnicodeInfo.GetCharInfo(code)) };

            // Search by description
            s_index ??= BuildUnicodeIndex();

            var result = new List<CharInfoWrapper>();
            search = search.ToUpperInvariant();
            foreach (var entry in s_index)
            {
                if (entry.Text.Contains(search, StringComparison.Ordinal))
                {
                    result.Add(new CharInfoWrapper(entry.CharInfo));
                    if (result.Count > 100)
                        break;
                }
            }

            return result;
        }

        private static List<IndexEntry> BuildUnicodeIndex()
        {
            var sw = Stopwatch.StartNew();
            var result = new List<IndexEntry>();
            var blocks = UnicodeInfo.GetBlocks();

            foreach (var block in blocks)
            {
                foreach (var codepoint in block.CodePointRange)
                {
                    if (char.IsSurrogate((char)codepoint))
                    {
                        continue;
                    }

                    var charInfo = UnicodeInfo.GetCharInfo(codepoint);
                    var displayText = charInfo.Name;
                    if (displayText != null)
                    {
                        result.Add(new(charInfo, displayText, displayText.ToUpperInvariant()));
                    }
                }
            }

            Console.WriteLine("Index built in " + sw.Elapsed);
            return result;
        }

        private record IndexEntry(UnicodeCharInfo CharInfo, string Text, string SearchText);
    }

    public record CharInfoWrapper(UnicodeCharInfo CharInfo)
    {
        public string DisplayValue => new Rune(CharInfo.CodePoint).ToString();
        public string DisplayCodePoint => "U+" + CharInfo.CodePoint.ToString("X4", CultureInfo.InvariantCulture);
        public string Category => CharInfo.Category.ToString();
        public string Block => CharInfo.Block;
        public string Name => CharInfo.Name;
        public string Escape => GetEscapeString(CharInfo.CodePoint);

        private static string GetEscapeString(int value)
        {
            if (char.ConvertFromUtf32(value).Length == 2)
            {
                return "\\U" + value.ToString("X", CultureInfo.InvariantCulture).PadLeft(8, '0');
            }
            else
            {
                return "\\u" + value.ToString("X", CultureInfo.InvariantCulture).PadLeft(4, '0');
            }
        }
    }
}
