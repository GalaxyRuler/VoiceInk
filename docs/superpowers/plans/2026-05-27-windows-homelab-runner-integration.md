# Windows Homelab Runner Integration Plan

## Goal

Register VoiceInk Windows with the central Homelab runner for safe metadata/route planning while keeping GUI/MSIX/App Installer install smoke in project-owned disposable Windows CI lanes.

## Steps

1. Inspect existing project Homelab metadata and central Homelab runner scripts.
2. Add `.codex/homelab-runner.json` with `container` and `headless` allowed classes.
3. Add dry-run-only profiles under `qa/profiles`.
4. Add `qa/homelab/README.md`, `qa/vm/README.md`, and `AGENTS.md` guidance.
5. Register the project with `Register-CodexRunnerProject.ps1`.
6. Validate integration and resolve routes without executing jobs.
7. Update the project completion tracker.

## Verification

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Admin\Documents\Codex\Homelab\codex-isolated-test-runners\tools\codex-runner\Test-HomelabSupportsProject.ps1" -ProjectPath "<repo>" -Json
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Admin\Documents\Codex\Homelab\codex-isolated-test-runners\tools\codex-runner\Register-CodexRunnerProject.ps1" -ProjectPath "<repo>" -ProjectName "VoiceInk Windows" -Classes container,headless -DefaultIsolation container -Json
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Admin\Documents\Codex\Homelab\codex-isolated-test-runners\tools\codex-runner\Test-CodexRunnerProjectIntegration.ps1" -ProjectPath "<repo>" -Json
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Admin\Documents\Codex\Homelab\codex-isolated-test-runners\tools\codex-runner\Resolve-CodexRunnerRoute.ps1" -ProjectPath "<repo>" -Class container -Profile release-metadata -Json
powershell -NoProfile -ExecutionPolicy Bypass -File "C:\Users\Admin\Documents\Codex\Homelab\codex-isolated-test-runners\tools\codex-runner\Resolve-CodexRunnerRoute.ps1" -ProjectPath "<repo>" -Class headless -Profile windows-dotnet-cli -Json
```
