# Windows Dev ZIP Per-User Install Implementation Plan

## Goal

Advance packaging parity without requiring signing credentials or global machine changes.

## Task 1: Test Contract

- Add a packaging asset test that requires per-user install/uninstall scripts.
- Ensure the scripts default to plan-only mode and require `-Execute` for mutation.
- Ensure the scripts avoid MSIX install commands, certificate import commands, machine-wide install roots, and global registry roots.

## Task 2: Install Script

- Add `install-dev-zip.ps1`.
- Validate package path containment under `VoiceInk.Windows\artifacts`.
- Validate install root containment under `%LocalAppData%`.
- In `-Execute`, extract the dev ZIP, replace the bounded per-user install folder, and create a current-user Start Menu shortcut.

## Task 3: Uninstall Script

- Add `uninstall-dev-zip.ps1`.
- Validate install root containment under `%LocalAppData%`.
- In `-Execute`, remove the current-user Start Menu shortcut and bounded install folder.

## Task 4: Docs And Verification

- Update README and project completion.
- Run focused packaging tests, full tests, full Debug x64 build, and whitespace check.
- Commit the completed slice.

## Result

Completed on 2026-05-26 with per-user Dev ZIP install/uninstall helpers.
