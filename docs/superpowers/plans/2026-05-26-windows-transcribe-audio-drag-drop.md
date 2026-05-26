# Windows Transcribe Audio Drag And Drop Implementation Plan

## Task 1: XAML Drop Target

- [x] Enable `AllowDrop` on the Transcribe Audio section.
- [x] Wire `DragOver` and `Drop` handlers.

## Task 2: Drop Handler

- [x] Accept only `StorageItems` when the queue is editable.
- [x] Read dropped storage items with `GetStorageItemsAsync`.
- [x] Filter to `StorageFile` local paths.
- [x] Reuse the same queue helper as the picker path.

## Task 3: Docs And Verification

- [x] Update README and completion tracker.
- [x] Run full tests/build and commit.
