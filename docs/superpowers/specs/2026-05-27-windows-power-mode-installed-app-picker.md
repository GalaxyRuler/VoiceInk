# Windows Power Mode Installed App Picker

## Goal

Add a Windows-native installed app picker to the Power Mode editor so users can add app process targets without manually typing process names.

## Source Of Truth

- macOS VoiceInk lets users create app/site-specific Power Modes without needing to know platform internals.
- Windows Power Mode matching already uses process-name patterns and active-window process detection.
- Windows app discovery is split between Start menu shortcuts for Win32 apps and package identity for Store/MSIX apps.

## Online Grounding

- Microsoft documents `Get-AppxPackage` as the package enumeration surface for installed AppX/MSIX packages.
- Microsoft documents AUMID discovery for apps that appear in the Start menu.
- Start menu shortcut folders remain the practical local discovery surface for many Win32 apps, including per-user and all-user shortcuts.
- References:
  - https://learn.microsoft.com/powershell/module/appx/get-appxpackage
  - https://learn.microsoft.com/windows/configuration/store/find-aumid

## Requirements

- The Power Mode editor must include an installed app picker near the process target fields.
- Choosing an installed app must append its process name to the rule's process-pattern field without duplicating existing alternatives.
- Installed app choices must be normalized, sorted, and de-duplicated in Core.
- Native discovery must be best-effort and local-only; failure to inspect shortcuts must not break the Power Mode page.
- No registry writes, app launches, package mutations, or global machine changes.

## Non-Goals

- No uninstall/install management.
- No Store package launch or AUMID matching behavior in this slice.
- No icon extraction or rich app artwork.
