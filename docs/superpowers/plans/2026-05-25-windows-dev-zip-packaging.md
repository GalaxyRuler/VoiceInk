# Windows Dev ZIP Packaging Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a repeatable open-source-friendly Windows dev ZIP packaging path for the WinUI app, before the later MSIX/installer slice.

**Architecture:** Keep the app unpackaged for this slice and add a repo-local PowerShell packaging script that runs `dotnet publish` for `win-x64` with .NET self-contained output and `WindowsAppSDKSelfContained=true`, stages a README next to the app, and zips the published folder under an ignored artifacts directory. Do not add installer dependencies or signing requirements in this slice.

**Tech Stack:** .NET 10 SDK, WinUI 3 / Windows App SDK self-contained deployment, PowerShell `Compress-Archive`.

---

### Task 1: Packaging Script

**Files:**
- Create: `VoiceInk.Windows/scripts/package-dev-zip.ps1`
- Modify: `.gitignore`

- [x] **Step 1: Add generated artifact ignore**

Add:

```gitignore
VoiceInk.Windows/artifacts/
```

- [x] **Step 2: Add packaging script**

Create `VoiceInk.Windows/scripts/package-dev-zip.ps1` that:

- Resolves repo root and Windows root from `$PSScriptRoot`.
- Uses a `-DotNetPath` parameter, defaulting to `DOTNET_EXE`, then repo-local `..\.dotnet-sdk-10\dotnet.exe`, then `dotnet`.
- Publishes `VoiceInk.Windows/src/VoiceInk.Windows.App/VoiceInk.Windows.App.csproj`.
- Defaults to `Release`, `win-x64`, and `VoiceInk.Windows/artifacts/dev-zip`.
- Runs:

```powershell
dotnet publish <app-project> -c Release -r win-x64 --self-contained true -p:Platform=x64 -p:WindowsPackageType=None -p:WindowsAppSDKSelfContained=true -p:PublishSingleFile=false -o <publish-dir>
```

- Writes a small `VOICEINK-WINDOWS-README.txt` into the package folder explaining how to launch `VoiceInk.Windows.App.exe`, configure a Whisper GGML `.bin` model, and where app data is stored.
- Compresses the publish folder into `VoiceInk-Windows-dev-win-x64.zip`.
- Deletes only paths under the script-owned artifacts directory after verifying the resolved absolute target path is inside that directory.

- [x] **Step 3: Run script help/syntax check**

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File VoiceInk.Windows\scripts\package-dev-zip.ps1 -Help
```

Expected: help text or parameter output exits 0.

### Task 2: Documentation And Tracker

**Files:**
- Modify: `README.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Modify: `docs/superpowers/project-completion.md`
- Modify: this plan file

- [x] **Step 1: Update README**

Add a Windows packaging subsection with:

```powershell
.\VoiceInk.Windows\scripts\package-dev-zip.ps1 -DotNetPath "..\.dotnet-sdk-10\dotnet.exe"
```

Document output path, `.exe` launch, runtime/model prerequisites, and that MSIX/installer/signing remain later packaging work.

- [x] **Step 2: Update parity spec**

Add a Packaging slice completed note for the dev ZIP script and update Packaging gaps to leave MSIX/installer, signing, uninstall behavior, shortcuts, and installer smoke tests.

- [x] **Step 3: Update completion bar**

Increase Packaging from `8%` to a modest value that reflects a repeatable dev ZIP package while not overstating installer parity.

### Task 3: Verification, Review, Commit

**Files:**
- All files above

- [x] **Step 1: Run packaging script**

```powershell
.\VoiceInk.Windows\scripts\package-dev-zip.ps1 -DotNetPath "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe"
```

Expected: publish succeeds and creates `VoiceInk.Windows/artifacts/dev-zip/VoiceInk-Windows-dev-win-x64.zip`.

- [x] **Step 2: Verify package contents**

```powershell
$zip = "VoiceInk.Windows\artifacts\dev-zip\VoiceInk-Windows-dev-win-x64.zip"
if (!(Test-Path $zip)) { throw "ZIP missing: $zip" }
Add-Type -AssemblyName System.IO.Compression.FileSystem
[IO.Compression.ZipFile]::OpenRead((Resolve-Path $zip)).Entries | Select-Object -ExpandProperty FullName | Select-String "VoiceInk.Windows.App.exe"
```

Expected: ZIP contains `VoiceInk.Windows.App.exe`.

- [x] **Step 3: Run focused build**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [x] **Step 4: Request review and fix Critical/Important findings**

Review focus: script safety, path handling, self-contained publish properties, docs accuracy, generated artifacts ignored, no new dependencies.

- [x] **Step 5: Full verification and commit**

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
git add .gitignore VoiceInk.Windows\scripts README.md docs\superpowers
git diff --cached --check
git commit -m "build(windows): add dev zip packaging script"
```
