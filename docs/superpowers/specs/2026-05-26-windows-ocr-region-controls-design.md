# Windows OCR Region Controls Design

## Goal

Expose simple, visible OCR region controls so users can constrain Screen OCR Context without needing an overlay picker yet.

## External Grounding

Microsoft documents WinUI `NumberBox` as the native numeric input control for editable numeric values. The OCR region is four numeric screen-coordinate values, so `NumberBox` is a better fit than free-form text boxes.

## Behavior

- Keep Screen OCR Context default-off.
- Add an optional OCR region setting with left, top, width, and height.
- Show numeric region controls in the Enhancement page when OCR context is enabled.
- Persist the region controls in JSON settings.
- Use the saved region at OCR read time.
- Use full virtual-desktop OCR when region mode is off.
- Skip OCR recognition for empty or non-positive region dimensions.

## Non-Goals

- No overlay drag picker in this slice.
- No screenshot persistence.
- No OCR unless the existing Screen OCR Context setting is enabled.
