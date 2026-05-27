# Windows Context Desktop Screenshot Boundary Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Replace inaccurate Windows Graphics Capture consent/border wording with accurate desktop screenshot OCR privacy guidance.

**Architecture:** Keep the wording in `EnhancementContextReadinessPresenter`, where context rows are already presenter-backed and testable. No native capture code changes.

**Tech Stack:** C#, .NET 10, xUnit.

---

### Task 1: Add Failing Presenter Test

**Files:**
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

- [ ] **Step 1: Write failing test**

Add:

```csharp
[Fact]
public void Present_WithOcrContext_ShowsDesktopScreenshotCaptureBoundary()
{
    var presentation = EnhancementContextReadinessPresenter.Present(
        new AppSettings
        {
            UseOcrContext = true
        });

    Assert.Contains(
        presentation.PrivacyRows,
        row => row.Title == "Desktop Screenshot Capture"
            && row.Value == "Transient"
            && row.Detail == "Screen OCR takes a one-time desktop screenshot through the Windows desktop capture path, runs local OCR, then discards the image bytes."
            && row.StatusBadge == "Image local");
}
```

- [ ] **Step 2: Run test to verify it fails**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests.Present_WithOcrContext_ShowsDesktopScreenshotCaptureBoundary -nr:false -p:UseSharedCompilation=false
```

Expected: FAIL because the privacy row still uses Windows Capture Consent wording.

### Task 2: Implement Correct Privacy Row

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs`

- [ ] **Step 1: Replace OCR consent row**

Rename `OcrCaptureConsentRow` to a desktop screenshot boundary row and return:

```csharp
new(
    "Desktop Screenshot Capture",
    "Transient",
    "Screen OCR takes a one-time desktop screenshot through the Windows desktop capture path, runs local OCR, then discards the image bytes.",
    "Image local")
```

- [ ] **Step 2: Update existing tests**

Update tests that expected `Windows Capture Consent`.

- [ ] **Step 3: Run focused tests**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter EnhancementContextReadinessPresenterTests -nr:false -p:UseSharedCompilation=false
```

Expected: PASS.

### Task 3: Verify, Document, Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`

- [ ] **Step 1: Update docs**

Mention the corrected desktop screenshot OCR boundary in context parity.

- [ ] **Step 2: Run verification**

Run:

```powershell
& '..\.dotnet-sdk-10\dotnet.exe' test VoiceInk.Windows\VoiceInk.Windows.sln -nr:false -p:UseSharedCompilation=false
& '..\.dotnet-sdk-10\dotnet.exe' build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64 -nr:false -p:UseSharedCompilation=false
git diff --check
```

Expected: all commands succeed; `git diff --check` may show CRLF notices only.

- [ ] **Step 3: Commit**

Run:

```powershell
git add VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextReadinessPresenter.cs VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementContextReadinessPresenterTests.cs docs/superpowers/specs/2026-05-27-windows-context-desktop-screenshot-boundary.md docs/superpowers/plans/2026-05-27-windows-context-desktop-screenshot-boundary.md docs/superpowers/project-completion.md docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md
git commit -m "fix(windows): clarify context screenshot boundary"
```
