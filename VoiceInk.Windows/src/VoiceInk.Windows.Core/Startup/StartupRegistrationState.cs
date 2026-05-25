namespace VoiceInk.Windows.Core.Startup;

public sealed record StartupRegistrationState(
    bool IsEnabled,
    bool HasExternalValue,
    string? RegisteredCommand,
    string? ExpectedCommand)
{
    public static StartupRegistrationState Disabled(string? expectedCommand) =>
        new(false, false, null, expectedCommand);

    public static StartupRegistrationState Enabled(string registeredCommand, string expectedCommand) =>
        new(true, false, registeredCommand, expectedCommand);

    public static StartupRegistrationState ExternalValue(string registeredCommand, string expectedCommand) =>
        new(false, true, registeredCommand, expectedCommand);
}
