namespace VoiceInk.Windows.Core.Text;

public static class TextPostProcessor
{
    public static string Process(string text, TextPostProcessingOptions options)
    {
        var trimmed = text.Trim();

        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        return options.AppendTrailingSpace ? $"{trimmed} " : trimmed;
    }
}
