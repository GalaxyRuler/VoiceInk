using Microsoft.Win32;
using VoiceInk.Windows.Core.Services;
using VoiceInk.Windows.Core.Startup;

namespace VoiceInk.Windows.Native.Startup;

public sealed class RegistryStartupRegistrationService : IStartupRegistrationService
{
    private const string RunKeyPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "VoiceInk.Windows";

    private readonly string expectedCommand;

    public RegistryStartupRegistrationService()
        : this(Environment.ProcessPath)
    {
    }

    public RegistryStartupRegistrationService(string? executablePath)
    {
        expectedCommand = StartupLaunchCommand.Build(executablePath);
    }

    public Task<StartupRegistrationState> GetStateAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var runKey = Registry.CurrentUser.OpenSubKey(RunKeyPath, writable: false);
        var registeredCommand = runKey?.GetValue(ValueName) as string;
        if (string.IsNullOrWhiteSpace(registeredCommand))
        {
            return Task.FromResult(StartupRegistrationState.Disabled(expectedCommand));
        }

        if (string.Equals(registeredCommand, expectedCommand, StringComparison.Ordinal))
        {
            return Task.FromResult(StartupRegistrationState.Enabled(registeredCommand, expectedCommand));
        }

        return Task.FromResult(StartupRegistrationState.ExternalValue(registeredCommand, expectedCommand));
    }

    public Task SetEnabledAsync(bool isEnabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("VoiceInk could not open the Windows startup registry key.");
        var registeredCommand = runKey.GetValue(ValueName) as string;
        var hasExternalValue = !string.IsNullOrWhiteSpace(registeredCommand)
            && !string.Equals(registeredCommand, expectedCommand, StringComparison.Ordinal);

        if (isEnabled)
        {
            if (string.IsNullOrWhiteSpace(expectedCommand))
            {
                throw new InvalidOperationException("VoiceInk could not resolve the executable path for launch at login.");
            }

            if (hasExternalValue)
            {
                throw new InvalidOperationException("VoiceInk found a different Windows startup command. Reapply Launch at Login to replace it.");
            }

            runKey.SetValue(ValueName, expectedCommand, RegistryValueKind.String);
        }
        else
        {
            if (!hasExternalValue)
            {
                runKey.DeleteValue(ValueName, throwOnMissingValue: false);
            }
        }

        return Task.CompletedTask;
    }

    public Task RepairRegistrationAsync(bool isEnabled, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (isEnabled && string.IsNullOrWhiteSpace(expectedCommand))
        {
            throw new InvalidOperationException("VoiceInk could not resolve the executable path for launch at login.");
        }

        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("VoiceInk could not open the Windows startup registry key.");

        if (isEnabled)
        {
            runKey.SetValue(ValueName, expectedCommand, RegistryValueKind.String);
        }
        else
        {
            runKey.DeleteValue(ValueName, throwOnMissingValue: false);
        }

        return Task.CompletedTask;
    }

    public Task RestoreStateAsync(StartupRegistrationState state, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var runKey = Registry.CurrentUser.CreateSubKey(RunKeyPath, writable: true)
            ?? throw new InvalidOperationException("VoiceInk could not open the Windows startup registry key.");

        if ((state.IsEnabled || state.HasExternalValue)
            && !string.IsNullOrWhiteSpace(state.RegisteredCommand))
        {
            runKey.SetValue(ValueName, state.RegisteredCommand, RegistryValueKind.String);
        }
        else
        {
            runKey.DeleteValue(ValueName, throwOnMissingValue: false);
        }

        return Task.CompletedTask;
    }
}
