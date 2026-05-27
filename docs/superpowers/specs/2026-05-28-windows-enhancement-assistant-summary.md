# Windows Enhancement Assistant Summary Spec

## Intent

Make the Windows Enhancement page expose Assistant Mode state as a first-class summary, matching the original app's emphasis on switching between dictation cleanup and one-request assistant behavior.

## Requirements

- Add a presenter-backed Assistant Mode summary line to the Enhancement context presentation.
- Show available guidance when the normal dictation prompt is selected.
- Show selected guidance when the Assistant prompt is selected.
- Bind the summary in the WinUI Enhancement page with a stable UI Automation name.
- Keep enhancement provider behavior, prompt rendering, trigger-word activation, retries, and context collection unchanged.

## Open-Source Boundary

This is local UI guidance only. It adds no account flow, licensing, telemetry, paid provider default, or commercial gating.

## Verification

- Focused Core presenter tests.
- Focused XAML accessibility test.
- WinUI app build because the Enhancement page XAML changed.
