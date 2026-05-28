# Windows Disposable Runner Setup Packet

This packet prepares the external lane for the final VoiceInk Windows release evidence gate. It is for a disposable or resettable Windows machine, not active WHITEDRAGON.

References:

- GitHub self-hosted runner labels: https://docs.github.com/actions/hosting-your-own-runners/using-labels-with-self-hosted-runners
- GitHub self-hosted runners in workflows: https://docs.github.com/en/actions/how-tos/managing-self-hosted-runners/using-self-hosted-runners-in-a-workflow
- Windows App Certification Kit command line: https://learn.microsoft.com/en-us/windows/uwp/debug-test-perf/windows-app-certification-kit

## Target

- Repository: `GalaxyRuler/VoiceInk`
- Runner labels required by the workflow: `self-hosted`, `windows`, `voiceink-windows-qa`
- Recommended runner name: `voiceink-windows-qa-01`
- Staging root on the runner: `C:\VoiceInkQa`
- Runner working directory: `C:\VoiceInkQa\actions-runner`
- Release artifact staging directory: `C:\VoiceInkQa\artifacts`

## Non-Negotiable Boundaries

- Do not run GUI smoke, MSIX install/uninstall, WACK, or certificate trust mutation on active WHITEDRAGON.
- Do not paste registration tokens into committed files, docs, shell history you plan to share, or screenshots.
- Do not commit `.pfx`, `.cer`, passwords, registration tokens, generated runner credentials, or runner `_work` contents.
- Do not run as a Windows service for GUI evidence. Use `run.cmd` in an active user session so screenshot capture and App Certification Kit launch checks can see the desktop.

## Machine Prerequisites

1. Start from a disposable Windows VM or resettable physical test box.
2. Sign in to an interactive desktop session as the QA account.
3. Install current Windows updates and reboot if required.
4. Install GitHub Actions runner from the GitHub UI for `GalaxyRuler/VoiceInk`.
5. Install Windows App Certification Kit.
6. Put the maintainer-owned signing certificate in the appropriate Trusted People store before smoke execution.
7. Stage the real signed VoiceInk `.msix` under `C:\VoiceInkQa\artifacts`.

## Runner Registration

In GitHub, open:

```text
GalaxyRuler/VoiceInk -> Settings -> Actions -> Runners -> New self-hosted runner
```

Choose Windows and copy the current GitHub-generated download/config commands. Run them from an elevated PowerShell window on the disposable runner, but keep the registration token out of files.

Recommended fixed setup shape:

```powershell
$ErrorActionPreference = 'Stop'
$qaRoot = 'C:\VoiceInkQa'
$runnerRoot = Join-Path $qaRoot 'actions-runner'
$artifactRoot = Join-Path $qaRoot 'artifacts'
New-Item -ItemType Directory -Force -Path $runnerRoot, $artifactRoot | Out-Null
Set-Location $runnerRoot
```

After extracting the runner package, configure it with the GitHub-provided token and labels:

```powershell
.\config.cmd --url https://github.com/GalaxyRuler/VoiceInk --token <registration-token-from-github-ui> --name voiceink-windows-qa-01 --labels voiceink-windows-qa --work _work
```

GitHub automatically applies `self-hosted` and the OS label such as `windows`; the custom label is `voiceink-windows-qa`.

Start the runner in the visible desktop session:

```powershell
.\run.cmd
```

Do not close this window while the workflow is running.

## Prove The Runner

From another terminal with `gh` authenticated:

```powershell
gh api repos/GalaxyRuler/VoiceInk/actions/runners
```

Expected: a runner whose labels include `self-hosted`, `windows`, and `voiceink-windows-qa`, with status `online`.

If the API token cannot list runners, use the GitHub web UI:

```text
GalaxyRuler/VoiceInk -> Settings -> Actions -> Runners
```

## Prove WACK

On the disposable runner:

```powershell
$wackRoot = "${env:ProgramFiles(x86)}\Windows Kits\10\App Certification Kit"
$appCert = Join-Path $wackRoot 'appcert.exe'
Test-Path -LiteralPath $appCert
& $appCert /?
```

Expected: `Test-Path` returns `True`, and `appcert.exe /?` prints command-line help.

## Prove Signing Trust

Use maintainer-owned certificate handling outside the repository. The installed package signer must be trusted before the workflow runs. The project readiness docs use the Local Machine Trusted People store as the reference point:

```powershell
Get-ChildItem -LiteralPath 'Cert:\LocalMachine\TrustedPeople' |
  Select-Object Subject, Thumbprint, NotAfter
```

Expected: the package signing certificate subject matches the MSIX manifest publisher, currently `CN=VoiceInkOpenSource`.

## Stage The Signed Package

Copy the real signed MSIX into:

```text
C:\VoiceInkQa\artifacts\VoiceInk.Windows.signed.msix
```

Then prove it exists:

```powershell
Get-Item -LiteralPath 'C:\VoiceInkQa\artifacts\VoiceInk.Windows.signed.msix' |
  Select-Object FullName, Length, LastWriteTime
```

Use that exact path as `signed_package_path` when dispatching the workflow.

## Cleanup After Evidence

After collecting the evidence artifact, reset or destroy the disposable runner. If keeping the runner for another pass, remove any staged package, downloaded evidence, and temporary logs from `C:\VoiceInkQa\artifacts`.
