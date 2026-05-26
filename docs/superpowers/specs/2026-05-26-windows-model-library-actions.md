# Windows Model Library Actions

## Goal

Add scan-friendly model library action rows for local Whisper download, import, default selection, repair, and warmup state.

## Source Of Truth

VoiceInk's open-source local path depends on selecting or downloading a Whisper model and keeping transcription private on-device. The Windows fork already supports model import, catalog downloads, default selection, repair guidance, and warmup. This slice makes those actions easier to understand at a glance.

## Behavior

- Extend the Core model library overview with action rows.
- Rows summarize:
  - Catalog downloads.
  - Imported/custom models.
  - Default model selection.
  - Repair and warmup readiness.
- Derive rows from existing catalog items, imported model list, selected model path, and unavailable import count.
- Do not change download, import, default selection, repair, warmup, or persistence behavior.
- Keep model management local and open-source.

## UI

Render action rows under the existing Local Whisper Library overview on the AI Models page.

## Testing

Add presenter tests for:

- Empty library action rows.
- Populated/default library action rows.
- Unavailable imports repair row.
- Custom imported default row.

## Out Of Scope

- No new model download sources.
- No model deletion.
- No signed release/model bundle changes.
