namespace VoiceInk.Windows.Core.History;

public static class HistoryPlaybackRatePresenter
{
    public static IReadOnlyList<HistoryPlaybackRateChoice> Choices { get; } =
    [
        new("0.75x", 0.75),
        new("1.0x", 1.0),
        new("1.25x", 1.25),
        new("1.5x", 1.5),
        new("2.0x", 2.0)
    ];

    public static HistoryPlaybackRateChoice DefaultChoice { get; } = Choices[1];

    public static HistoryPlaybackRateChoice ChoiceAtOrDefault(int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex < Choices.Count
            ? Choices[selectedIndex]
            : DefaultChoice;

    public static int SelectedIndexFor(double value)
    {
        for (var index = 0; index < Choices.Count; index++)
        {
            if (Math.Abs(Choices[index].Value - value) < 0.001)
            {
                return index;
            }
        }

        return SelectedIndexFor(DefaultChoice.Value);
    }
}
