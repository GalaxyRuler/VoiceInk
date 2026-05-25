# Windows OCR Region Picker Design

## Goal

Add a visual screen-region picker for OCR context so users do not need to manually type capture coordinates.

## Source Grounding

Microsoft documents WinUI 3 secondary windows through `Window` and `AppWindow`, and full-screen windows through `FullScreenPresenter`. This slice uses a transient full-screen WinUI picker window and keeps OCR capture settings in the existing persisted `AppSettings` fields.

## Behavior

- Enhancement settings keep the existing numeric OCR region fields.
- A new `Select Region` action opens a transient full-screen overlay.
- Dragging on the overlay draws a visible selection rectangle.
- Releasing the pointer converts the selected rectangle to a `ScreenCaptureRegion` using the overlay window origin and current XAML rasterization scale.
- Tiny accidental drags are canceled and leave saved settings unchanged.
- Escape or closing the picker cancels without saving.
- A successful selection enables OCR context and constrained OCR region mode, fills left/top/width/height fields, and saves through the existing enhancement settings path.

## Non-Goals

- No screenshot preview or capture inside the picker.
- No multi-monitor stitched overlay beyond the full-screen display where the picker opens.
- No OCR execution during selection.

## Verification

- Native geometry tests cover drag normalization, display origin offsets, DPI scaling, and tiny-drag cancellation.
- Focused native/infrastructure tests pass.
- Windows app Debug x64 build passes.
- Full solution tests pass before commit.
