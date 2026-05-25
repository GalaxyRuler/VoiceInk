# Windows Selected Text Clipboard Fallback Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add a guarded clipboard-copy fallback for selected text enhancement context.

**Architecture:** Keep Core unchanged. Add small Native text-provider interfaces so `WindowsEnhancementContextProvider` can be tested without touching the real clipboard or keyboard. Implement fallback in Native using Windows Forms clipboard/send-keys APIs behind the interface.

**Tech Stack:** .NET 10, Windows Forms clipboard/send keys, UI Automation, xUnit.

---

### Task 1: Provider Interfaces And Failing Orchestration Tests

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/IClipboardTextReader.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ISelectedTextReader.cs`
- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ISelectedTextClipboardFallbackReader.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/ClipboardEnhancementContextProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/SelectedTextEnhancementContextProvider.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`
- Create: `VoiceInk.Windows/tests/VoiceInk.Windows.Infrastructure.Tests/Text/WindowsEnhancementContextProviderTests.cs`

Steps:

1. Write tests for fallback selected text, UIA-first behavior, and clipboard restore-before-context ordering.
2. Run the focused tests and confirm they fail because injectable fallback interfaces do not exist.
3. Add the interfaces and update existing providers to implement them.
4. Update `WindowsEnhancementContextProvider` to call fallback only after UIA returns empty.
5. Run the focused tests and confirm they pass.

### Task 2: Native Clipboard Fallback

**Files:**

- Create: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/SelectedTextClipboardFallbackReader.cs`
- Modify: `VoiceInk.Windows/src/VoiceInk.Windows.Native/Text/WindowsEnhancementContextProvider.cs`

Steps:

1. Implement a fallback reader that captures current clipboard data, sends Ctrl+C, waits briefly, reads text, truncates to 4,000 chars, and restores the prior clipboard in a finally block.
2. Use an STA helper for Windows Forms clipboard operations.
3. Swallow non-cancellation errors and return empty text.
4. Wire it into the default `WindowsEnhancementContextProvider` constructor.

### Task 3: Docs, Verification, And Commit

**Files:**

- Modify: `docs/superpowers/project-completion.md`
- Modify: `docs/superpowers/specs/2026-05-24-windows-open-source-parity-design.md`
- Add: `docs/superpowers/specs/2026-05-25-windows-selected-text-clipboard-fallback-design.md`
- Add: `docs/superpowers/plans/2026-05-25-windows-selected-text-clipboard-fallback.md`

Verification:

```powershell
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\tests\VoiceInk.Windows.Infrastructure.Tests\VoiceInk.Windows.Infrastructure.Tests.csproj --filter "FullyQualifiedName~WindowsEnhancementContextProviderTests"
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" test VoiceInk.Windows\VoiceInk.Windows.sln
& "C:\Users\Admin\Documents\Codex\2026-05-24\how-can-we-make-this-app\VoiceInk\.worktrees\.dotnet-sdk-10\dotnet.exe" build VoiceInk.Windows\VoiceInk.Windows.sln -c Debug -p:Platform=x64
git diff --check
```

Commit:

```powershell
git commit -m "feat(windows): add selected text clipboard fallback"
```
