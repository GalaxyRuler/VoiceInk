# Windows Signed MSIX Smoke Plan Design

## Goal

Add a repo-local signed MSIX smoke helper that gives maintainers a repeatable install/query/uninstall sequence while remaining non-mutating by default.

## Source Grounding

Microsoft documents MSIX management through PowerShell cmdlets: `Add-AppxPackage` installs signed `.msix` packages, `Get-AppxPackage` queries installed packages, and `Remove-AppxPackage` removes them. Microsoft also documents that signed packages must be trusted on the target device; self-signed test certificates require a separate tester/admin trust step. This slice does not create certificates, import certificates, trust certificates, build packages, or sign packages.

## Behavior

- Add `VoiceInk.Windows/scripts/smoke-msix-install.ps1`.
- The script accepts `-PackagePath`, optional `-PackageName`, and `-Execute`.
- Without `-Execute`, the script only validates inputs and prints the exact manual smoke commands.
- With `-Execute`, the script:
  - verifies the package path is a `.msix` file under `VoiceInk.Windows/artifacts`;
  - runs `Add-AppxPackage -Path`;
  - runs `Get-AppxPackage -Name`;
  - removes the exact installed package via `Remove-AppxPackage -Package <PackageFullName>`.
- The script refuses package paths outside `VoiceInk.Windows/artifacts`.
- The script refuses reparse-point package paths inside artifacts.
- The script prints the certificate trust prerequisite but never imports certificates.

## Non-Goals

- No certificate generation or import.
- No package signing.
- No automatic admin elevation.
- No package build.
- No app launch smoke.

## Verification

- Static packaging tests cover script presence, default plan mode, execute gate, artifact path safety, reparse-point safety, and absence of certificate import/generation.
- Script `-Help` runs.
- Script plan mode runs against a synthetic artifact-local `.msix` file without installing.
- Focused packaging tests, full solution tests, and Debug x64 build pass before commit.
