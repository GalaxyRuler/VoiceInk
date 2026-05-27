namespace VoiceInk.Windows.Core.Recording;

public sealed record RecordingSoundModeChoice(string Mode, string DisplayName);

public static class RecordingSoundModeSettings
{
    public const string SystemDefault = "systemDefault";
    public const string Asterisk = "asterisk";
    public const string Beep = "beep";
    public const string Exclamation = "exclamation";
    public const string Hand = "hand";
    public const string Question = "question";
    public const string Custom = "custom";

    public static IReadOnlyList<RecordingSoundModeChoice> SelectableModes { get; } =
    [
        new(SystemDefault, "System Default"),
        new(Asterisk, "Asterisk"),
        new(Beep, "Beep"),
        new(Exclamation, "Exclamation"),
        new(Hand, "Hand"),
        new(Question, "Question"),
        new(Custom, "Custom Sound")
    ];

    public static string Normalize(string? mode)
    {
        var trimmed = mode?.Trim();
        return SelectableModes.FirstOrDefault(choice =>
                string.Equals(choice.Mode, trimmed, StringComparison.OrdinalIgnoreCase))
            ?.Mode
            ?? SystemDefault;
    }

    public static bool IsBuiltInWindowsSound(string? mode)
    {
        var normalized = Normalize(mode);
        return normalized is Asterisk or Beep or Exclamation or Hand or Question;
    }
}
