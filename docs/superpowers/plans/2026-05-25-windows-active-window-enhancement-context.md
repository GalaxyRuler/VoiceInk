# Windows Active Window Enhancement Context Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add best-effort active-window process/title context to enhancement prompts.

**Architecture:** Extend Core `EnhancementContext` and `EnhancementContextRequest` with active-window fields. Reuse the Native Power Mode foreground-window provider from `WindowsEnhancementContextProvider`.

**Tech Stack:** .NET 10, WinUI 3, Win32 foreground-window APIs via existing Native Power Mode provider, xUnit.

---

### Task 1: Core Context Rendering

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContext.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementContextRequest.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Core/Enhancement/EnhancementPromptRenderer.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Core.Tests/Enhancement/EnhancementPromptTests.cs`

Steps:

1. Add a failing renderer test that expects `<ACTIVE_WINDOW_CONTEXT>` with process/title before selected text, clipboard, and vocabulary.
2. Add `ActiveWindowProcessName` and `ActiveWindowTitle` to `EnhancementContext`.
3. Add `IncludeActiveWindow = true` to `EnhancementContextRequest`.
4. Render active-window context before selected text.
5. Run the focused renderer test.

### Task 2: Windows Provider Wiring

**Files:**

- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`
- Modify: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsEnhancementContextProviderTests.cs`

Steps:

1. Add a failing provider test using a fake `IPowerModeTargetProvider`.
2. Inject an `IPowerModeTargetProvider` into `WindowsEnhancementContextProvider`.
3. Use `ActiveWindowPowerModeTargetProvider` in the default constructor.
4. On success, copy target process/title into `EnhancementContext`.
5. Swallow non-cancellation failures and return empty active-window context.
6. Run focused Windows context-provider tests.

### Task 3: Docs, Verification, And Commit

Files:

- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Add: `docs/superpowers/specs/2026-05-25-windows-active-window-enhancement-context-design.md`
- Add: `docs/superpowers/plans/2026-05-25-windows-active-window-enhancement-context.md`

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Commit:

```powershell
git commit -m "feat(windows): add active window enhancement context"
```
