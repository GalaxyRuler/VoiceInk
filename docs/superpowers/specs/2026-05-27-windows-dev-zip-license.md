# Windows Dev ZIP License File

## Goal

Close a packaging compliance gap by including the repository GPL license text in the unpackaged Windows dev ZIP.

## Source Of Truth

- VoiceInk for Windows is an open-source GPL fork.
- The repository root already contains `LICENSE`.
- Binary/dev ZIP recipients should receive a copy of the license text alongside the application README.

## Online Grounding

- GNU's GPL how-to recommends including a copy of the license itself somewhere in the distribution, commonly as a plain text `COPYING` file: https://www.gnu.org/licenses/gpl-howto.html

## Requirements

- `package-dev-zip.ps1` copies the repository root `LICENSE` into the package as `LICENSE.txt`.
- `test-dev-zip.ps1` validates `LICENSE.txt` exists and contains GPL text.
- Packaging tests cover both scripts.
- Keep dev ZIP install/uninstall behavior unchanged.

## Acceptance

- Focused packaging tests fail before license packaging validation exists.
- Focused tests pass after implementation.
- Full solution tests and Debug x64 build pass.
