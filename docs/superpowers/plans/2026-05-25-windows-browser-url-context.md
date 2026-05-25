# Windows Browser URL Context Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add sanitized, best-effort active browser URL context to AI enhancement prompts.

**Architecture:** Keep context data and URL sanitization in Core. Keep Windows UI Automation behind a Native reader interface so failures stay isolated from enhancement.

**Tech Stack:** .NET 10, WinUI 3, System.Windows.Automation, xUnit.

---

### Task 1: Core Browser URL Context

**Files:**
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContext.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextRequest.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/BrowserUrlContextSanitizer.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/TextEnhancementPipelineTests.cs`

- [ ] **Step 1: Write failing core tests**

Add tests for sanitized browser URLs, rejected unsafe/non-web URLs, prompt rendering order, and default pipeline request behavior.

- [ ] **Step 2: Run focused core tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Core.Tests\VoiceInk.Windows.Core.Tests.csproj --filter "FullyQualifiedName~Enhancement"
```

Expected: compile failure because the new browser URL members do not exist.

- [ ] **Step 3: Implement minimal Core support**

Add `BrowserUrl` to `EnhancementContext`, `IncludeBrowserUrl` to `EnhancementContextRequest`, a sanitizer, and a `<BROWSER_URL_CONTEXT>` prompt section.

- [ ] **Step 4: Run focused core tests and verify green**

Run the same focused command. Expected: enhancement tests pass.

### Task 2: Windows Browser URL Reader

**Files:**
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IBrowserUrlReader.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/BrowserUrlEnhancementContextProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsEnhancementContextProviderTests.cs`

- [ ] **Step 1: Write failing provider tests**

Add tests proving requested browser URL context is included, skipped when not requested, and skipped when the reader throws.

- [ ] **Step 2: Run focused infrastructure tests and verify red**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~WindowsEnhancementContextProviderTests"
```

Expected: compile failure because `IBrowserUrlReader` and the provider constructor overload do not exist.

- [ ] **Step 3: Implement Native reader and provider integration**

Use UI Automation to inspect edit controls in the foreground browser window. Return the first sanitized HTTP/HTTPS URL and return empty string on unsupported browser processes, timeouts, UIA failures, or unsafe URLs.

- [ ] **Step 4: Run focused infrastructure tests and verify green**

Run the same focused infrastructure command. Expected: provider tests pass.

### Task 3: Full Verification and Commit

**Files:**
- Modify: `docs/superpowers/project-completion.md`

- [ ] **Step 1: Run full tests**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
```

- [ ] **Step 2: Run Debug x64 build**

Run:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
```

- [ ] **Step 3: Run whitespace check**

Run:

```powershell
git diff --check
```

- [ ] **Step 4: Update completion tracker and commit**

Commit only intentional source/docs changes.
