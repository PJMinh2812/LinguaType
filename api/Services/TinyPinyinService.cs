using System.Text;
using ToolGood.Words.Pinyin;

namespace LinguaType.Api.Services;

public sealed class ToneMarkedPinyinService : IPinyinService
{
    public string ConvertToPinyin(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var raw = WordsHelper.GetPinyin(text, true);
        return PostProcessToneMarkedPinyin(raw);
    }

    private static string PostProcessToneMarkedPinyin(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(value.Length + 8);
        var previousWasLetter = false;

        foreach (var ch in value)
        {
            if (char.IsWhiteSpace(ch))
            {
                if (builder.Length > 0 && builder[^1] != ' ')
                {
                    builder.Append(' ');
                }

                previousWasLetter = false;
                continue;
            }

            if (char.IsUpper(ch) && previousWasLetter && builder.Length > 0 && builder[^1] != ' ')
            {
                builder.Append(' ');
            }

            builder.Append(char.ToLowerInvariant(ch));
            previousWasLetter = char.IsLetter(ch);
        }

        return builder.ToString().Trim();
    }
}
