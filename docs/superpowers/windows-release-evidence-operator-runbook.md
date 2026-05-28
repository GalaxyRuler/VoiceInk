# Windows Release Evidence Operator Runbook

This runbook executes the final external evidence gate for VoiceInk Windows 100% release readiness. It assumes source-owned checks already pass and the disposable Windows runner setup packet has been completed.

Do not run this on active WHITEDRAGON. MSIX install/uninstall, WACK, GUI launch, screenshot capture, and certificate trust setup belong only on the disposable/self-hosted Windows runner.

## Inputs

- Repository: `GalaxyRuler/VoiceInk`
- Workflow: `windows-installer-smoke.yml`
- Ref: `main`
- Runner label: `voiceink-windows-qa`
- Signed package path on runner: example `C:\VoiceInkQa\artifacts\VoiceInk.Windows.signed.msix`
- Evidence artifact name: `installer-smoke-evidence`

## 1. Confirm Local Source Readiness

From the repo root:

```powershell
.\VoiceInk.Windows\scripts\test-release-readiness.ps1
.\VoiceInk.Windows\scripts\show-project-completion.ps1
```

Expected: release readiness passes, and project completion still reports the external signed MSIX install/WACK/GUI smoke gate.

## 2. Confirm Dispatch Readiness

Run the non-mutating readiness helper:

```powershell
.\VoiceInk.Windows\scripts\test-installer-dispatch-readiness.ps1 `
  -Owner GalaxyRuler `
  -Repo VoiceInk `
  -Workflow windows-installer-smoke.yml `
  -Ref main `
  -RunnerLabel voiceink-windows-qa `
  -SignedPackagePath 'C:\VoiceInkQa\artifacts\VoiceInk.Windows.signed.msix'
```

If your token cannot list runners but the GitHub UI proves the runner is online, rerun without `-RequireRunner` and keep the UI screenshot or note with the release evidence. If you require API proof, add `-RequireRunner`.

## 3. Dispatch The Installer/WACK/GUI Workflow

Use the command printed by the readiness helper. The shape is:

```powershell
gh workflow run windows-installer-smoke.yml `
  --repo GalaxyRuler/VoiceInk `
  --ref main `
  -f runner_label=voiceink-windows-qa `
  -f signed_package_path='C:\VoiceInkQa\artifacts\VoiceInk.Windows.signed.msix' `
  -f main_package_uri='https://example.invalid/VoiceInk.Windows_0.1.0.0_x64.msix' `
  -f appinstaller_path='VoiceInk.Windows\artifacts\gha-installer-smoke\VoiceInk.Windows.appinstaller' `
  -f wack_report_path='VoiceInk.Windows\artifacts\gha-installer-smoke\wack-report.xml' `
  -f execute_install_smoke=true `
  -f run_wack=true `
  -f run_gui_smoke=true
```

The runner must have an active user session. Leave `run.cmd` open until the workflow finishes.

## 4. Watch The Run

```powershell
gh run list --repo GalaxyRuler/VoiceInk --workflow windows-installer-smoke.yml --limit 5
gh run watch --repo GalaxyRuler/VoiceInk <run-id>
```

If WACK or install smoke fails, keep the uploaded evidence artifact. The workflow uploads evidence with `always()` so the failure is still useful.

## 5. Download Evidence

```powershell
$evidenceRoot = 'VoiceInk.Windows\artifacts\downloaded-installer-smoke-evidence'
New-Item -ItemType Directory -Force -Path $evidenceRoot | Out-Null
gh run download <run-id> --repo GalaxyRuler/VoiceInk --name installer-smoke-evidence --dir $evidenceRoot
```

Expected files include:

- `installer-smoke-summary.txt`
- `wack-report.xml`
- `gui-smoke-log.txt`
- `gui-smoke-window.json`
- `gui-smoke-screenshot.png`

## 6. Validate Evidence

```powershell
.\VoiceInk.Windows\scripts\test-installer-smoke-evidence.ps1 `
  -EvidenceRoot 'VoiceInk.Windows\artifacts\downloaded-installer-smoke-evidence' `
  -RequireInstallSmoke `
  -RequireWackReport `
  -RequireGuiSmoke
```

Expected: `Installer smoke evidence validation passed.`

## 7. Release Gate Result

The release evidence gate is complete only when:

- signed MSIX install/uninstall smoke passed;
- WACK produced a parseable `wack-report.xml`;
- GUI smoke produced `gui-smoke-log.txt`, `gui-smoke-window.json`, and `gui-smoke-screenshot.png`;
- `test-installer-smoke-evidence.ps1 -RequireInstallSmoke -RequireWackReport -RequireGuiSmoke` passed against the downloaded artifact.

Do not mark VoiceInk Windows 100% before those evidence files exist and validate.
