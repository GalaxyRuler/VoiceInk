# Windows History Detail Accessibility Plan

## Slice

Add explicit UI Automation names for embedded History detail controls.

## Steps

1. Add a focused XAML accessibility test for the History search box, transcript text boxes, playback rate picker, and audio player.
2. Confirm the test fails before XAML changes.
3. Add `AutomationProperties.Name` attributes to the target controls.
4. Run focused XAML accessibility tests.
5. Build the WinUI app project to verify the XAML compiles.
6. Update the completion tracker and commit the slice.

## Verification

- Focused red run failed because six History controls lacked explicit automation names.
- Focused green run passed after implementation:
  - `dotnet test VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/VoiceInk.Windows.Core.Tests.csproj --filter FullyQualifiedName~AppXamlAccessibilityTests`
