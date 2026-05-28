# Windows Project Completion Reporter Plan

## Slice

Add a read-only completion reporter script.

## Steps

1. Add a failing packaging asset test that expects the reporter script, bar output terms, external release gate language, and no install/certificate mutation commands.
2. Add `show-project-completion.ps1`.
3. Run the focused packaging asset test.
4. Run the script directly to verify output.
5. Update the completion tracker and commit the slice.

## Verification

- Focused red run failed because `show-project-completion.ps1` did not exist.
- Focused green run passed after adding the script.
- Manual script run printed:
  - `VoiceInk Windows parity  [####################] 99%`
  - current slice
  - signed MSIX/WACK/GUI smoke external release gate.
