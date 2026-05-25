# Windows OCR Capture Region Contract Design

## Goal

Prepare Screen OCR Context for picker/region controls by adding a testable capture-region contract below the UI layer.

## Behavior

- Keep OCR context default-off.
- Let OCR capture run against the whole virtual desktop when no region is configured.
- Let callers provide a screen capture region with left/top/width/height coordinates.
- Treat empty or negative-size regions as no capture.
- Keep OCR text recognition and max-character behavior unchanged.

## Non-Goals

- No visible region picker overlay in this slice.
- No screenshot persistence.
- No OCR without the existing `UseOcrContext` setting.
