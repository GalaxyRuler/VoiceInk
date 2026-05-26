namespace VoiceInk.Windows.Core.PowerMode;

public static class PowerModeAutoSendKeyPresenter
{
    public static IReadOnlyList<PowerModeAutoSendKeyChoice> Choices { get; } =
    [
        new(PowerModeAutoSendKey.None, "None"),
        new(PowerModeAutoSendKey.Enter, "Return"),
        new(PowerModeAutoSendKey.ShiftEnter, "Shift + Return"),
        new(PowerModeAutoSendKey.CommandEnter, "Ctrl + Return")
    ];

    public static int SelectedIndexFor(PowerModeAutoSendKey key)
    {
        var index = Choices.ToList().FindIndex(choice => choice.Key == key);
        return index >= 0 ? index : 0;
    }

    public static PowerModeAutoSendKey KeyForSelectedIndex(int selectedIndex) =>
        selectedIndex >= 0 && selectedIndex < Choices.Count
            ? Choices[selectedIndex].Key
            : PowerModeAutoSendKey.None;
}
