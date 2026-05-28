# Windows Project Completion Reporter

## Goal

Keep the project completion bar visible and repeatable outside the chat by adding a read-only script that prints the tracked overall parity bar, current slice, and external release gate.

## Behavior

- `VoiceInk.Windows/scripts/show-project-completion.ps1` reads `docs/superpowers/project-completion.md`.
- It prints the `VoiceInk Windows parity` bar exactly as tracked in the document.
- It prints the current slice block.
- It prints the explicit external release gate for signed MSIX install, WACK, and GUI smoke evidence on a disposable Windows runner.
- It supports `-Help`.
- It does not install, launch, sign, mutate certificates, or run GUI smoke.

## Rationale

The chat progress bar is useful while work is active, but a repo-local command gives future local and runner sessions a single source of truth for progress without relying on conversational context.
