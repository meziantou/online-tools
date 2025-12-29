using System.Unicode;

namespace Meziantou.OnlineTools.Utils;

public sealed record CharInfoWrapper(UnicodeCharInfo CharInfo)
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
